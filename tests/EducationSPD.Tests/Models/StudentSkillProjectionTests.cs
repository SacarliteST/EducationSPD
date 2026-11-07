using EducationSPD.Application.Episodes;
using EducationSPD.Domain.Episodes;
using EducationSPD.Domain.Models;
using EducationSPD.Domain.Skills;
using EducationSPD.Infrastructure.Episodes;
using EducationSPD.Infrastructure.Models;
using EducationSPD.Infrastructure.Skills;
using EducationSPD.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Tests.Models;

public sealed class StudentSkillProjectionTests(PostgresDatabaseFixture database)
    : IClassFixture<PostgresDatabaseFixture>
{
    [Fact]
    public async Task FirstObservation_StartsFromInitialMasteryProbability()
    {
        await database.ResetSkillsAsync();
        var skill = await AddSkillAsync("sql", "join");
        var studentId = Guid.NewGuid();
        var finalizedAt = new DateTimeOffset(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);

        await ApplyAsync(CreateFinalization(
            studentId,
            "sql",
            finalizedAt,
            skill.SkillId,
            SkillObservationOutcome.Succeeded));

        await using var dbContext = database.CreateDbContext();
        var state = await dbContext.StudentSkillStates.AsNoTracking().SingleAsync();
        var expected = BktCalculator.Calculate(
            0.2m,
            SkillObservationOutcome.Succeeded,
            SkillObservationSource.ValidationRule,
            ModelVersionTests.CreateParameters());
        Assert.Equal(expected, state.MasteryProbability, 12);
        Assert.Equal(1, state.ObservationsCount);
        Assert.Equal(finalizedAt, state.LastObservedAt);
        Assert.Equal(1, state.ModelVersion);
    }

    [Fact]
    public async Task LateEpisode_RebuildsProjectionInPedagogicalOrder()
    {
        await database.ResetSkillsAsync();
        var skill = await AddSkillAsync("sql", "group-by");
        var studentId = Guid.NewGuid();
        var earlier = new DateTimeOffset(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);
        var later = earlier.AddMinutes(5);

        await ApplyAsync(CreateFinalization(
            studentId,
            "sql",
            later,
            skill.SkillId,
            SkillObservationOutcome.Failed));
        await ApplyAsync(CreateFinalization(
            studentId,
            "sql",
            earlier,
            skill.SkillId,
            SkillObservationOutcome.Succeeded));

        var parameters = ModelVersionTests.CreateParameters();
        var afterSuccess = BktCalculator.Calculate(
            parameters.InitialMasteryProbability,
            SkillObservationOutcome.Succeeded,
            SkillObservationSource.ValidationRule,
            parameters);
        var expected = BktCalculator.Calculate(
            afterSuccess,
            SkillObservationOutcome.Failed,
            SkillObservationSource.ValidationRule,
            parameters);

        await using var dbContext = database.CreateDbContext();
        var state = await dbContext.StudentSkillStates.AsNoTracking().SingleAsync();
        Assert.Equal(expected, state.MasteryProbability, 12);
        Assert.Equal(2, state.ObservationsCount);
        Assert.Equal(later, state.LastObservedAt);
    }

    [Fact]
    public async Task Projections_AreIsolatedByStudentModuleAndSkill()
    {
        await database.ResetSkillsAsync();
        var join = await AddSkillAsync("sql", "join");
        var aggregate = await AddSkillAsync("sql", "aggregate");
        var variables = await AddSkillAsync("python", "variables");
        var firstStudent = Guid.NewGuid();
        var secondStudent = Guid.NewGuid();
        var finalizedAt = new DateTimeOffset(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);

        await ApplyAsync(CreateFinalization(
            firstStudent,
            "sql",
            finalizedAt,
            join.SkillId,
            SkillObservationOutcome.Succeeded,
            aggregate.SkillId));
        await ApplyAsync(CreateFinalization(
            secondStudent,
            "sql",
            finalizedAt,
            join.SkillId,
            SkillObservationOutcome.Failed));
        await ApplyAsync(CreateFinalization(
            firstStudent,
            "python",
            finalizedAt,
            variables.SkillId,
            SkillObservationOutcome.Succeeded));

        await using var dbContext = database.CreateDbContext();
        var states = await dbContext.StudentSkillStates.AsNoTracking().ToListAsync();
        Assert.Equal(4, states.Count);
        Assert.Contains(states, state => state.StudentId == firstStudent
            && state.ModuleKey == "sql"
            && state.SkillId == join.Id);
        Assert.Contains(states, state => state.StudentId == firstStudent
            && state.ModuleKey == "sql"
            && state.SkillId == aggregate.Id);
        Assert.Contains(states, state => state.StudentId == secondStudent
            && state.ModuleKey == "sql"
            && state.SkillId == join.Id);
        Assert.Contains(states, state => state.StudentId == firstStudent
            && state.ModuleKey == "python"
            && state.SkillId == variables.Id);
    }

    private async Task ApplyAsync(LearningEpisodeFinalization finalization)
    {
        await using var dbContext = database.CreateDbContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var handler = new LearningEpisodeFinalizationHandler(
            new EfLearningEpisodeRepository(dbContext),
            new EfSkillRepository(dbContext),
            new PostgresStudentModuleLock(dbContext),
            new EfModelVersionRepository(dbContext),
            new EfStudentSkillStateRepository(dbContext),
            TimeProvider.System);

        var result = await handler.ApplyAsync(finalization, CancellationToken.None);

        Assert.Equal(LearningEpisodeApplyResult.Applied, result);
        await transaction.CommitAsync();
    }

    private async Task<Skill> AddSkillAsync(string moduleKey, string skillId)
    {
        var skill = Skill.Create(
            Guid.NewGuid(),
            moduleKey,
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

    private static LearningEpisodeFinalization CreateFinalization(
        Guid studentId,
        string moduleKey,
        DateTimeOffset finalizedAt,
        string skillId,
        SkillObservationOutcome outcome,
        string? secondSkillId = null)
    {
        var skillResults = new List<LearningEpisodeSkillResult>
        {
            new(skillId, SkillObservationSource.ValidationRule, outcome),
        };
        if (secondSkillId is not null)
        {
            skillResults.Add(new(
                secondSkillId,
                SkillObservationSource.TaskTag,
                SkillObservationOutcome.Failed));
        }

        return new LearningEpisodeFinalization(
            Guid.NewGuid(),
            Guid.NewGuid(),
            studentId,
            moduleKey,
            Guid.NewGuid(),
            finalizedAt,
            skillResults);
    }
}
