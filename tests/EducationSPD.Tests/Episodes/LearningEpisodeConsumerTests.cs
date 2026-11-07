using EducationSPD.Domain.Inbox;
using EducationSPD.Domain.Skills;
using EducationSPD.Kafka.Contracts;
using EducationSPD.Kafka.Contracts.V1;
using EducationSPD.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Tests.Episodes;

public sealed class LearningEpisodeConsumerTests(KafkaCatalogFixture fixture)
    : IClassFixture<KafkaCatalogFixture>
{
    [Fact]
    public async Task Consumer_AppliesEpisodeIdempotentlyAndPreservesUnresolvedMessage()
    {
        var joinSkill = await AddSkillAsync("join");
        var groupBySkill = await AddSkillAsync("group-by");
        await fixture.StartEpisodeConsumerAsync();
        await fixture.StartEpisodeProcessorAsync();

        var episodeId = Guid.NewGuid();
        var appliedEvent = CreateEvent(
            Guid.NewGuid(),
            episodeId,
            new SkillResult("join", SkillResultSource.ValidationRule, SkillOutcome.Succeeded),
            new SkillResult("group-by", SkillResultSource.TaskTag, SkillOutcome.Failed));
        var appliedPayload = KafkaContractJson.Serialize(appliedEvent);
        await PublishAsync(appliedPayload, appliedEvent.StudentId);
        await WaitForInboxStatusAsync(appliedEvent.EventId, InboxStatus.Applied);

        await using (var appliedContext = fixture.CreateDbContext())
        {
            var episode = await appliedContext.LearningEpisodes
                .Include(item => item.Observations)
                .AsNoTracking()
                .SingleAsync(item => item.EventId == appliedEvent.EventId);
            Assert.Equal(2, episode.Observations.Count);
            Assert.Contains(episode.Observations, observation => observation.SkillId == joinSkill.Id);
            Assert.Contains(episode.Observations, observation => observation.SkillId == groupBySkill.Id);
        }

        await PublishAsync(appliedPayload, appliedEvent.StudentId);
        var conflictingEvent = CreateEvent(
            Guid.NewGuid(),
            episodeId,
            new SkillResult("join", SkillResultSource.ValidationRule, SkillOutcome.Failed));
        await PublishAsync(KafkaContractJson.Serialize(conflictingEvent), conflictingEvent.StudentId);
        await WaitForInboxStatusAsync(conflictingEvent.EventId, InboxStatus.Failed);

        var unresolvedEvent = CreateEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new SkillResult("join", SkillResultSource.ValidationRule, SkillOutcome.Succeeded),
            new SkillResult("window-functions", SkillResultSource.ValidationRule, SkillOutcome.Failed));
        await PublishAsync(KafkaContractJson.Serialize(unresolvedEvent), unresolvedEvent.StudentId);
        await WaitForInboxStatusAsync(unresolvedEvent.EventId, InboxStatus.PendingCatalog);

        await using var verificationContext = fixture.CreateDbContext();
        Assert.Equal(1, await verificationContext.LearningEpisodes.CountAsync());
        Assert.Equal(2, await verificationContext.SkillObservations.CountAsync());
        Assert.Equal(1, await verificationContext.InboxMessages.CountAsync(
            message => message.EventId == appliedEvent.EventId));
        var conflict = await verificationContext.InboxMessages.AsNoTracking()
            .SingleAsync(message => message.EventId == conflictingEvent.EventId);
        Assert.Equal("EpisodeConflict", conflict.ErrorCode);
        var unresolved = await verificationContext.InboxMessages.AsNoTracking()
            .SingleAsync(message => message.EventId == unresolvedEvent.EventId);
        Assert.Null(unresolved.ErrorCode);
    }

    private async Task<Skill> AddSkillAsync(string skillId)
    {
        var skill = Skill.Create(
            Guid.NewGuid(),
            "sql",
            skillId,
            skillId,
            null,
            EducationSPD.Domain.Skills.SkillOrigin.System,
            false,
            DateTimeOffset.UtcNow,
            Guid.NewGuid());
        await using var dbContext = fixture.CreateDbContext();
        dbContext.Skills.Add(skill);
        await dbContext.SaveChangesAsync();
        return skill;
    }

    private async Task PublishAsync(string payload, Guid studentId)
    {
        await fixture.PublishAsync(
            fixture.EpisodeFinalizedTopic,
            $"sql:{studentId:D}",
            payload);
    }

    private async Task WaitForInboxStatusAsync(Guid eventId, InboxStatus expectedStatus)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(15);
        while (DateTimeOffset.UtcNow < deadline)
        {
            await using var dbContext = fixture.CreateDbContext();
            if (await dbContext.InboxMessages.AnyAsync(
                    message => message.EventId == eventId && message.Status == expectedStatus))
            {
                return;
            }

            await Task.Delay(100);
        }

        Assert.Fail("Ожидаемое состояние обработки эпизода не достигнуто за 15 секунд.");
    }

    private static LearningEpisodeFinalized CreateEvent(
        Guid eventId,
        Guid episodeId,
        params SkillResult[] skillResults)
    {
        return new LearningEpisodeFinalized(
            KafkaSchemaVersions.Version1,
            eventId,
            episodeId,
            Guid.NewGuid(),
            "sql",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            skillResults);
    }
}
