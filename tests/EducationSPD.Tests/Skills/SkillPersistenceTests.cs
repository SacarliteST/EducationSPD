using EducationSPD.Domain.Skills;
using EducationSPD.Infrastructure.Persistence;
using EducationSPD.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EducationSPD.Tests.Skills;

public sealed class SkillPersistenceTests(PostgresDatabaseFixture database)
    : IClassFixture<PostgresDatabaseFixture>
{
    [Fact]
    public async Task SameModuleAndSkillId_CannotBeInsertedTwice()
    {
        await database.ResetSkillsAsync();
        await using var dbContext = database.CreateDbContext();
        dbContext.Skills.Add(CreateSkill(Guid.NewGuid(), "JOIN"));
        dbContext.Skills.Add(CreateSkill(Guid.NewGuid(), "JOIN повторно"));

        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            () => dbContext.SaveChangesAsync());

        var postgresException = Assert.IsType<PostgresException>(exception.InnerException);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, postgresException.SqlState);
    }

    [Fact]
    public async Task OlderCatalogState_DoesNotOverwriteStoredSkill()
    {
        await database.ResetSkillsAsync();
        var skill = CreateSkill(Guid.NewGuid(), "Актуальное название");
        await SaveAsync(skill);

        await using (var updateContext = database.CreateDbContext())
        {
            var stored = await updateContext.Skills.SingleAsync();
            var wasApplied = stored.ApplyChange(
                "Устаревшее название",
                null,
                SkillOrigin.TeacherDefined,
                true,
                stored.OccurredAt.AddMinutes(-1),
                Guid.NewGuid());

            Assert.False(wasApplied);
            await updateContext.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateDbContext();
        var persisted = await verificationContext.Skills.AsNoTracking().SingleAsync();
        Assert.Equal("Актуальное название", persisted.Name);
        Assert.False(persisted.IsDeleted);
    }

    [Fact]
    public async Task NewerCatalogState_UpdatesNameAndSoftDeletesWithoutChangingIdentity()
    {
        await database.ResetSkillsAsync();
        var skill = CreateSkill(Guid.NewGuid(), "Исходное название");
        var originalId = skill.Id;
        await SaveAsync(skill);

        await using (var updateContext = database.CreateDbContext())
        {
            var stored = await updateContext.Skills.SingleAsync();
            var wasApplied = stored.ApplyChange(
                "Новое название",
                "Навык больше нельзя назначать новым заданиям",
                SkillOrigin.TeacherDefined,
                true,
                stored.OccurredAt.AddMinutes(1),
                Guid.NewGuid());

            Assert.True(wasApplied);
            await updateContext.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateDbContext();
        var updated = await verificationContext.Skills.AsNoTracking().SingleAsync();
        Assert.Equal(originalId, updated.Id);
        Assert.Equal("Новое название", updated.Name);
        Assert.True(updated.IsDeleted);
        Assert.Equal(1, await verificationContext.Skills.CountAsync());
    }

    [Fact]
    public async Task Database_RejectsUnknownSkillOrigin()
    {
        await database.ResetSkillsAsync();
        await using var dbContext = database.CreateDbContext();
        var exception = await Assert.ThrowsAsync<PostgresException>(
            () => InsertRawSkillAsync(dbContext, "sql", 99));

        Assert.Equal(PostgresErrorCodes.CheckViolation, exception.SqlState);
    }

    [Fact]
    public async Task Database_RejectsModuleKeyLongerThanLimit()
    {
        await database.ResetSkillsAsync();
        await using var dbContext = database.CreateDbContext();
        var exception = await Assert.ThrowsAsync<PostgresException>(
            () => InsertRawSkillAsync(dbContext, new string('a', Skill.ModuleKeyMaxLength + 1), 1));

        Assert.Equal(PostgresErrorCodes.StringDataRightTruncation, exception.SqlState);
    }

    private async Task SaveAsync(Skill skill)
    {
        await using var dbContext = database.CreateDbContext();
        dbContext.Skills.Add(skill);
        await dbContext.SaveChangesAsync();
    }

    private static Skill CreateSkill(Guid id, string name)
    {
        return Skill.Create(
            id,
            "sql",
            "join",
            name,
            "Соединение данных нескольких таблиц",
            SkillOrigin.System,
            false,
            new DateTimeOffset(2026, 11, 7, 12, 0, 0, TimeSpan.Zero),
            Guid.NewGuid());
    }

    private static Task<int> InsertRawSkillAsync(
        AppDbContext dbContext,
        string moduleKey,
        short origin)
    {
        return dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "Skills"
                (id, module_key, skill_id, name, description, origin, is_deleted, occurred_at, source_event_id)
            VALUES
                ({Guid.NewGuid()}, {moduleKey}, {"join"}, {"JOIN"}, {null}, {origin}, {false}, {DateTimeOffset.UtcNow}, {Guid.NewGuid()});
            """);
    }
}
