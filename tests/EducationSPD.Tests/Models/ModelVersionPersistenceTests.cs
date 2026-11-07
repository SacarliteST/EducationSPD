using EducationSPD.Domain.Models;
using EducationSPD.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EducationSPD.Tests.Models;

public sealed class ModelVersionPersistenceTests(PostgresDatabaseFixture database)
    : IClassFixture<PostgresDatabaseFixture>
{
    [Fact]
    public async Task Migration_CreatesExactlyOneActiveVersion()
    {
        await using var dbContext = database.CreateDbContext();

        var activeVersions = await dbContext.ModelVersions
            .AsNoTracking()
            .Where(version => version.Status == ModelVersionStatus.Active)
            .ToListAsync();

        var active = Assert.Single(activeVersions);
        Assert.Equal(1, active.Version);
        Assert.Equal(16, active.AhpMatrix.Count);
        Assert.Equal(1m, active.AhpWeights.Sum(), 8);
    }

    [Fact]
    public async Task Database_DoesNotAllowSecondActiveVersion()
    {
        await using var dbContext = database.CreateDbContext();
        var duplicate = ModelVersion.CreateDraft(
            Guid.NewGuid(),
            2,
            ModelVersionTests.CreateParameters(),
            new DateTimeOffset(2026, 11, 7, 12, 0, 0, TimeSpan.Zero));
        duplicate.StartRecalculation(duplicate.CreatedAt);
        duplicate.Activate(duplicate.CreatedAt);
        dbContext.ModelVersions.Add(duplicate);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());

        var postgresException = Assert.IsType<PostgresException>(exception.InnerException);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, postgresException.SqlState);
    }
}
