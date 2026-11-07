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
        InboxMessageType messageType,
        Func<IReadOnlyList<InboxMessage>, CancellationToken, Task> processBatch,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var now = timeProvider.GetUtcNow();
        var receivedStatus = (short)InboxStatus.Received;
        var pendingCatalogStatus = (short)InboxStatus.PendingCatalog;
        var storedMessageType = (short)messageType;
        var messages = messageType == InboxMessageType.LearningEpisodeFinalized
            ? await LoadEpisodeBatchAsync(
                storedMessageType,
                receivedStatus,
                pendingCatalogStatus,
                now,
                cancellationToken)
            : await LoadRegularBatchAsync(
                storedMessageType,
                receivedStatus,
                pendingCatalogStatus,
                now,
                cancellationToken);

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

    private Task<List<InboxMessage>> LoadRegularBatchAsync(
        short messageType,
        short receivedStatus,
        short pendingCatalogStatus,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        return dbContext.InboxMessages
            .FromSqlInterpolated($"""
                SELECT *
                FROM "InboxMessages"
                WHERE message_type = {messageType}
                  AND status IN ({receivedStatus}, {pendingCatalogStatus})
                  AND (next_retry_at IS NULL OR next_retry_at <= {now})
                ORDER BY received_at, event_id
                LIMIT {options.BatchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);
    }

    private Task<List<InboxMessage>> LoadEpisodeBatchAsync(
        short messageType,
        short receivedStatus,
        short pendingCatalogStatus,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        return dbContext.InboxMessages
            .FromSqlInterpolated($"""
                SELECT current_message.*
                FROM "InboxMessages" AS current_message
                WHERE current_message.message_type = {messageType}
                  AND current_message.status IN ({receivedStatus}, {pendingCatalogStatus})
                  AND (current_message.next_retry_at IS NULL OR current_message.next_retry_at <= {now})
                  AND NOT EXISTS (
                      SELECT 1
                      FROM "InboxMessages" AS earlier_message
                      WHERE earlier_message.message_type = {messageType}
                        AND earlier_message.status IN ({receivedStatus}, {pendingCatalogStatus})
                        AND earlier_message.payload->>'moduleKey' = current_message.payload->>'moduleKey'
                        AND earlier_message.payload->>'studentId' = current_message.payload->>'studentId'
                        AND (
                            COALESCE(earlier_message.payload->>'finalizedAt', ''),
                            COALESCE(earlier_message.payload->>'episodeId', ''))
                          < (
                            COALESCE(current_message.payload->>'finalizedAt', ''),
                            COALESCE(current_message.payload->>'episodeId', '')))
                ORDER BY
                    COALESCE(current_message.payload->>'finalizedAt', ''),
                    COALESCE(current_message.payload->>'episodeId', ''),
                    current_message.received_at,
                    current_message.event_id
                LIMIT {options.BatchSize}
                FOR UPDATE OF current_message SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);
    }
}
