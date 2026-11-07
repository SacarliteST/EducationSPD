using EducationSPD.Domain.Inbox;
using EducationSPD.Domain.Skills;
using EducationSPD.Kafka.Contracts;
using EducationSPD.Kafka.Contracts.V1;
using EducationSPD.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Tests.Episodes;

public sealed class PendingCatalogQueueTests(KafkaCatalogFixture fixture)
    : IClassFixture<KafkaCatalogFixture>
{
    [Fact]
    public async Task PendingCatalog_BlocksOnlySameStudentAndResumesAfterCatalogUpdate()
    {
        await AddSkillAsync("join");
        await fixture.StartConsumerAsync();
        await fixture.StartProcessorAsync();
        await fixture.StartEpisodeConsumerAsync();
        await fixture.StartEpisodeProcessorAsync();

        var firstStudentId = Guid.NewGuid();
        var secondStudentId = Guid.NewGuid();
        var baseTime = DateTimeOffset.UtcNow.AddMinutes(-1);
        var waitingEvent = CreateEpisode(
            firstStudentId,
            baseTime,
            "window-functions");
        var laterEvent = CreateEpisode(
            firstStudentId,
            baseTime.AddSeconds(1),
            "join");
        var independentEvent = CreateEpisode(
            secondStudentId,
            baseTime.AddMilliseconds(500),
            "join");

        await PublishEpisodeAsync(waitingEvent);
        await PublishEpisodeAsync(laterEvent);
        await PublishEpisodeAsync(independentEvent);

        await WaitForStatusAsync(waitingEvent.EventId, InboxStatus.PendingCatalog);
        await WaitForStatusAsync(independentEvent.EventId, InboxStatus.Applied);
        await WaitForInboxAsync(laterEvent.EventId);

        await using (var blockedContext = fixture.CreateDbContext())
        {
            var blocked = await blockedContext.InboxMessages.AsNoTracking()
                .SingleAsync(message => message.EventId == laterEvent.EventId);
            Assert.Equal(InboxStatus.Received, blocked.Status);
            var waiting = await blockedContext.InboxMessages.AsNoTracking()
                .SingleAsync(message => message.EventId == waitingEvent.EventId);
            Assert.Equal(1, waiting.RetryCount);
        }

        var catalogEvent = new SkillCatalogChanged(
            KafkaSchemaVersions.Version1,
            Guid.NewGuid(),
            "sql",
            "window-functions",
            "Оконные функции",
            null,
            EducationSPD.Kafka.Contracts.V1.SkillOrigin.System,
            false,
            DateTimeOffset.UtcNow);
        await fixture.PublishAsync(
            "sql:window-functions",
            KafkaContractJson.Serialize(catalogEvent));

        await WaitForStatusAsync(catalogEvent.EventId, InboxStatus.Applied);
        await WaitForStatusAsync(waitingEvent.EventId, InboxStatus.Applied);
        await WaitForStatusAsync(laterEvent.EventId, InboxStatus.Applied);

        var exhaustedEvent = CreateEpisode(
            Guid.NewGuid(),
            baseTime.AddSeconds(2),
            "missing-forever");
        await PublishEpisodeAsync(exhaustedEvent);
        await WaitForStatusAsync(exhaustedEvent.EventId, InboxStatus.Failed, TimeSpan.FromSeconds(15));

        await using var verificationContext = fixture.CreateDbContext();
        Assert.Equal(3, await verificationContext.LearningEpisodes.CountAsync());
        var exhausted = await verificationContext.InboxMessages.AsNoTracking()
            .SingleAsync(message => message.EventId == exhaustedEvent.EventId);
        Assert.Equal("CatalogRetryLimitExceeded", exhausted.ErrorCode);
        Assert.Equal(9, exhausted.RetryCount);
    }

    private async Task AddSkillAsync(string skillId)
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
    }

    private async Task PublishEpisodeAsync(LearningEpisodeFinalized message)
    {
        await fixture.PublishAsync(
            fixture.EpisodeFinalizedTopic,
            $"sql:{message.StudentId:D}",
            KafkaContractJson.Serialize(message));
    }

    private async Task WaitForInboxAsync(Guid eventId)
    {
        await WaitUntilAsync(async dbContext =>
            await dbContext.InboxMessages.AnyAsync(message => message.EventId == eventId),
            TimeSpan.FromSeconds(5));
    }

    private async Task WaitForStatusAsync(
        Guid eventId,
        InboxStatus status,
        TimeSpan? timeout = null)
    {
        await WaitUntilAsync(
            async dbContext => await dbContext.InboxMessages.AnyAsync(
                message => message.EventId == eventId && message.Status == status),
            timeout ?? TimeSpan.FromSeconds(5));
    }

    private async Task WaitUntilAsync(
        Func<EducationSPD.Infrastructure.Persistence.AppDbContext, Task<bool>> condition,
        TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        while (DateTimeOffset.UtcNow < deadline)
        {
            await using var dbContext = fixture.CreateDbContext();
            if (await condition(dbContext))
            {
                return;
            }

            await Task.Delay(100);
        }

        Assert.Fail($"Ожидаемое состояние не достигнуто за {timeout.TotalSeconds} секунд.");
    }

    private static LearningEpisodeFinalized CreateEpisode(
        Guid studentId,
        DateTimeOffset finalizedAt,
        string skillId)
    {
        return new LearningEpisodeFinalized(
            KafkaSchemaVersions.Version1,
            Guid.NewGuid(),
            Guid.NewGuid(),
            studentId,
            "sql",
            Guid.NewGuid(),
            finalizedAt,
            [new SkillResult(skillId, SkillResultSource.ValidationRule, SkillOutcome.Succeeded)]);
    }
}
