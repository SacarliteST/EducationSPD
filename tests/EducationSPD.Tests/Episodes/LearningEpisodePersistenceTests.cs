using EducationSPD.Domain.Episodes;
using EducationSPD.Domain.Skills;
using EducationSPD.Infrastructure.Episodes;
using EducationSPD.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Tests.Episodes;

public sealed class LearningEpisodePersistenceTests(PostgresDatabaseFixture database)
    : IClassFixture<PostgresDatabaseFixture>
{
    [Fact]
    public async Task Episode_PersistsMinimalHistoryAndRestrictsSkillDeletion()
    {
        var skill = await AddSkillAsync("join");
        var episode = CreateEpisode(Guid.NewGuid(), Guid.NewGuid(), skill.Id);

        await using (var dbContext = database.CreateDbContext())
        {
            var repository = new EfLearningEpisodeRepository(dbContext);
            await repository.AddAsync(episode, CancellationToken.None);
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        await using (var readContext = database.CreateDbContext())
        {
            var repository = new EfLearningEpisodeRepository(readContext);
            var stored = await repository.FindByEventIdAsync(
                episode.EventId,
                CancellationToken.None);

            Assert.NotNull(stored);
            Assert.Equal(episode.EpisodeId, stored.EpisodeId);
            Assert.Equal(episode.StudentId, stored.StudentId);
            Assert.Equal(episode.TaskId, stored.TaskId);
            var observation = Assert.Single(stored.Observations);
            Assert.Equal(skill.Id, observation.SkillId);
            Assert.Equal(SkillObservationSource.ValidationRule, observation.Source);
            Assert.Equal(SkillObservationOutcome.Succeeded, observation.Outcome);
        }

        await using var deleteContext = database.CreateDbContext();
        var storedSkill = await deleteContext.Skills.SingleAsync(item => item.Id == skill.Id);
        deleteContext.Skills.Remove(storedSkill);
        await Assert.ThrowsAsync<DbUpdateException>(() => deleteContext.SaveChangesAsync());
    }

    [Fact]
    public async Task Database_RejectsDuplicateEventAndEpisodeIdentity()
    {
        var skill = await AddSkillAsync("group-by");
        var eventId = Guid.NewGuid();
        var episodeId = Guid.NewGuid();
        await AddEpisodeAsync(CreateEpisode(eventId, episodeId, skill.Id));

        await AssertDatabaseRejectsAsync(CreateEpisode(eventId, Guid.NewGuid(), skill.Id));
        await AssertDatabaseRejectsAsync(CreateEpisode(Guid.NewGuid(), episodeId, skill.Id));
    }

    private async Task<Skill> AddSkillAsync(string skillId)
    {
        var skill = Skill.Create(
            Guid.NewGuid(),
            "sql",
            skillId,
            skillId,
            null,
            SkillOrigin.System,
            false,
            DateTimeOffset.UtcNow,
            Guid.NewGuid());

        await using var dbContext = database.CreateDbContext();
        dbContext.Skills.Add(skill);
        await dbContext.SaveChangesAsync();
        return skill;
    }

    private async Task AddEpisodeAsync(LearningEpisode episode)
    {
        await using var dbContext = database.CreateDbContext();
        dbContext.LearningEpisodes.Add(episode);
        await dbContext.SaveChangesAsync();
    }

    private async Task AssertDatabaseRejectsAsync(LearningEpisode episode)
    {
        await using var dbContext = database.CreateDbContext();
        dbContext.LearningEpisodes.Add(episode);
        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    private static LearningEpisode CreateEpisode(
        Guid eventId,
        Guid episodeId,
        Guid skillId)
    {
        return LearningEpisode.Create(
            Guid.NewGuid(),
            eventId,
            episodeId,
            Guid.NewGuid(),
            "sql",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            1,
            [
                new NewSkillObservation(
                    Guid.NewGuid(),
                    skillId,
                    SkillObservationSource.ValidationRule,
                    SkillObservationOutcome.Succeeded)
            ]);
    }
}
