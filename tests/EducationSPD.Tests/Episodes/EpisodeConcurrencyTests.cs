using EducationSPD.Application.Episodes;
using EducationSPD.Domain.Episodes;
using EducationSPD.Domain.Skills;
using EducationSPD.Infrastructure.Episodes;
using EducationSPD.Infrastructure.Skills;
using EducationSPD.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Tests.Episodes;

public sealed class EpisodeConcurrencyTests(PostgresDatabaseFixture database)
    : IClassFixture<PostgresDatabaseFixture>
{
    [Fact]
    public async Task TransactionLock_SerializesOneStudentWithoutBlockingAnother()
    {
        var skill = await AddSkillAsync();
        var firstStudentId = Guid.NewGuid();
        var secondStudentId = Guid.NewGuid();
        var baseTime = DateTimeOffset.UtcNow.AddMinutes(-1);

        await using var firstContext = database.CreateDbContext();
        await using var firstTransaction = await firstContext.Database.BeginTransactionAsync();
        var firstResult = await CreateHandler(firstContext).ApplyAsync(
            CreateFinalization(firstStudentId, baseTime.AddSeconds(1), skill.SkillId),
            CancellationToken.None);
        Assert.Equal(LearningEpisodeApplyResult.Applied, firstResult);

        await using var blockedContext = database.CreateDbContext();
        await using var blockedTransaction = await blockedContext.Database.BeginTransactionAsync();
        var lateEpisodeId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var blockedApply = CreateHandler(blockedContext).ApplyAsync(
            CreateFinalization(firstStudentId, baseTime, skill.SkillId, lateEpisodeId),
            CancellationToken.None);

        var prematureCompletion = await Task.WhenAny(blockedApply, Task.Delay(300));
        Assert.NotSame(blockedApply, prematureCompletion);

        await using (var independentContext = database.CreateDbContext())
        await using (var independentTransaction = await independentContext.Database.BeginTransactionAsync())
        {
            var independentResult = await CreateHandler(independentContext).ApplyAsync(
                CreateFinalization(secondStudentId, baseTime, skill.SkillId),
                CancellationToken.None);
            Assert.Equal(LearningEpisodeApplyResult.Applied, independentResult);
            await independentTransaction.CommitAsync();
        }

        await firstTransaction.CommitAsync();
        Assert.Equal(LearningEpisodeApplyResult.Applied, await blockedApply.WaitAsync(TimeSpan.FromSeconds(3)));
        await blockedTransaction.CommitAsync();

        await using var verificationContext = database.CreateDbContext();
        var repository = new EfLearningEpisodeRepository(verificationContext);
        var history = await repository.ListChronologicalAsync(
            firstStudentId,
            "sql",
            CancellationToken.None);
        Assert.Equal(2, history.Count);
        Assert.Equal(lateEpisodeId, history[0].EpisodeId);
        Assert.True(history[0].FinalizedAt < history[1].FinalizedAt);
        Assert.Equal(3, await verificationContext.SkillObservations.CountAsync());
    }

    private async Task<Skill> AddSkillAsync()
    {
        var skill = Skill.Create(
            Guid.NewGuid(),
            "sql",
            "join",
            "JOIN",
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

    private static LearningEpisodeFinalizationHandler CreateHandler(
        EducationSPD.Infrastructure.Persistence.AppDbContext dbContext)
    {
        return new LearningEpisodeFinalizationHandler(
            new EfLearningEpisodeRepository(dbContext),
            new EfSkillRepository(dbContext),
            new PostgresStudentModuleLock(dbContext));
    }

    private static LearningEpisodeFinalization CreateFinalization(
        Guid studentId,
        DateTimeOffset finalizedAt,
        string skillId,
        Guid? episodeId = null)
    {
        return new LearningEpisodeFinalization(
            Guid.NewGuid(),
            episodeId ?? Guid.NewGuid(),
            studentId,
            "sql",
            Guid.NewGuid(),
            finalizedAt,
            [
                new LearningEpisodeSkillResult(
                    skillId,
                    SkillObservationSource.ValidationRule,
                    SkillObservationOutcome.Succeeded)
            ]);
    }
}
