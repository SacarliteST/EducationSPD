using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Infrastructure.Persistence.Migrations;

internal sealed class DatabaseMigrationManager : IDatabaseMigrationManager
{
    private readonly AppDbContext dbContext;

    public DatabaseMigrationManager(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Database.MigrateAsync(cancellationToken);
    }
}
