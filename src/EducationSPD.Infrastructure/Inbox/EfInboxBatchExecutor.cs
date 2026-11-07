using EducationSPD.Application.Inbox;
using EducationSPD.Domain.Inbox;
using EducationSPD.Infrastructure.Configuration;
using EducationSPD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EducationSPD.Infrastructure.Inbox;

internal sealed class EfInboxBatchExecutor : IInboxBatchExecutor
{
    private readonly AppDbContext dbContext;
    private readonly InboxOptions options;
    private readonly TimeProvider timeProvider;

    public EfInboxBatchExecutor(
        AppDbContext dbContext,
        IOptions<InboxOptions> options,
        TimeProvider timeProvider)
    {
        this.dbContext = dbContext;
        this.options = options.Value;
        this.timeProvider = timeProvider;
    }

    public async Task<int> ExecuteReadyBatchAsync(
        Func<IReadOnlyList<InboxMessage>, CancellationToken, Task> processBatch,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var now = timeProvider.GetUtcNow();
        var receivedStatus = (short)InboxStatus.Received;
        var pendingCatalogStatus = (short)InboxStatus.PendingCatalog;
        var messages = await dbContext.InboxMessages
            .FromSqlInterpolated($"""
                SELECT *
                FROM "InboxMessages"
                WHERE status IN ({receivedStatus}, {pendingCatalogStatus})
                  AND (next_retry_at IS NULL OR next_retry_at <= {now})
                ORDER BY received_at, event_id
                LIMIT {options.BatchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return 0;
        }

        await processBatch(messages, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return messages.Count;
    }
}
