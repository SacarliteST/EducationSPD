using EducationSPD.Application.Episodes;
using EducationSPD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Infrastructure.Episodes;

internal sealed class PostgresStudentModuleLock(AppDbContext dbContext)
    : IStudentModuleLock
{
    public async Task AcquireAsync(
        Guid studentId,
        string moduleKey,
        CancellationToken cancellationToken)
    {
        if (dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "Блокировка истории студента должна выполняться внутри транзакции.");
        }

        var lockKey = $"{moduleKey}:{studentId:D}";
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0));",
            cancellationToken);
    }
}
