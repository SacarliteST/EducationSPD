using EducationSPD.Domain.Episodes;
using EducationSPD.Domain.Models;
using EducationSPD.Domain.Skills;
using EducationSPD.Infrastructure.Configuration;
using EducationSPD.Infrastructure.Episodes;
using EducationSPD.Infrastructure.Models;
using EducationSPD.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EducationSPD.Tests.Models;

public sealed class ModelRecalculationTests(PostgresDatabaseFixture database)
    : IClassFixture<PostgresDatabaseFixture>
{
    [Fact]
    public async Task Recalculation_ResumesFromCursorAndAtomicallyActivatesCompleteVersion()
    {
        await database.ResetSkillsAsync();
        var sourceState = await SeedHistoryAndDraftAsync();

        await ExecuteBatchAsync();

        await using (var partialContext = database.CreateDbContext())
        {
            var versions = await partialContext.ModelVersions.AsNoTracking().ToListAsync();
            var active = Assert.Single(versions, version => version.Status == ModelVersionStatus.Active);
            var recalculating = Assert.Single(
                versions,
                version => version.Status == ModelVersionStatus.Recalculating);
            var partialStates = await partialContext.StudentSkillStates
                .AsNoTracking()
                .Where(state => state.ModelVersion == 2)
                .ToListAsync();

            Assert.Equal(1, active.Version);
            Assert.Equal(2, recalculating.Version);
            Assert.NotNull(recalculating.RecalculationCursorModuleKey);
            Assert.NotNull(recalculating.RecalculationCursorStudentId);
            Assert.Single(partialStates);
        }

        await ExecuteBatchAsync();
        await ExecuteBatchAsync();

        await using var completedContext = database.CreateDbContext();
        var completedVersions = await completedContext.ModelVersions.AsNoTracking().ToListAsync();
        var newActive = Assert.Single(
            completedVersions,
            version => version.Status == ModelVersionStatus.Active);
        var archived = Assert.Single(
            completedVersions,
            version => version.Status == ModelVersionStatus.Archived);
        var rebuiltStates = await completedContext.StudentSkillStates
            .AsNoTracking()
            .Where(state => state.ModelVersion == 2)
            .ToListAsync();
        var sourceAfterRecalculation = await completedContext.LearningEpisodes
            .AsNoTracking()
            .Include(episode => episode.Observations)
            .OrderBy(episode => episode.StudentId)
            .ToListAsync();

        Assert.Equal(2, newActive.Version);
        Assert.Equal(1, archived.Version);
        Assert.Equal(2, rebuiltStates.Count);
        Assert.Equal(sourceState, DescribeSource(sourceAfterRecalculation));
    }

    private async Task<IReadOnlyList<string>> SeedHistoryAndDraftAsync()
    {
        var now = new DateTimeOffset(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);
        var skill = Skill.Create(
            Guid.NewGuid(),
            "sql",
            "join",
            "Соединение таблиц",
            null,
            SkillOrigin.System,
            false,
            now,
            Guid.NewGuid());
        var episodes = new[]
        {
            CreateEpisode(Guid.NewGuid(), skill.Id, now, SkillObservationOutcome.Succeeded),
            CreateEpisode(Guid.NewGuid(), skill.Id, now.AddMinutes(1), SkillObservationOutcome.Failed),
        };
        var changedParameters = new ModelParameters(
            0.3m,
            0.1m,
            0.1m,
            0.1m,
            0.25m,
            0.25m,
            ModelVersionTests.CreateParameters().AhpMatrix);
        var draft = ModelVersion.CreateDraft(Guid.NewGuid(), 2, changedParameters, now.AddMinutes(2));

        await using var dbContext = database.CreateDbContext();
        dbContext.Skills.Add(skill);
        dbContext.LearningEpisodes.AddRange(episodes);
        dbContext.ModelVersions.Add(draft);
        await dbContext.SaveChangesAsync();
        return DescribeSource(episodes);
    }

    private async Task ExecuteBatchAsync()
    {
        await using var dbContext = database.CreateDbContext();
        var executor = new EfModelRecalculationBatchExecutor(
            dbContext,
            new PostgresStudentModuleLock(dbContext),
            Options.Create(new RecalculationOptions { BatchSize = 1, PollIntervalSeconds = 1 }),
            TimeProvider.System,
            NullLogger<EfModelRecalculationBatchExecutor>.Instance);

        Assert.True(await executor.ExecuteAsync(CancellationToken.None));
    }

    private static LearningEpisode CreateEpisode(
        Guid studentId,
        Guid skillId,
        DateTimeOffset finalizedAt,
        SkillObservationOutcome outcome) =>
        LearningEpisode.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            studentId,
            "sql",
            Guid.NewGuid(),
            finalizedAt,
            1,
            [new NewSkillObservation(Guid.NewGuid(), skillId, SkillObservationSource.ValidationRule, outcome)]);

    private static IReadOnlyList<string> DescribeSource(IEnumerable<LearningEpisode> episodes) =>
        episodes
            .OrderBy(episode => episode.StudentId)
            .SelectMany(episode => episode.Observations.Select(observation =>
                $"{episode.EventId}:{episode.EpisodeId}:{episode.StudentId}:{observation.Id}:{observation.Outcome}"))
            .ToArray();
}
