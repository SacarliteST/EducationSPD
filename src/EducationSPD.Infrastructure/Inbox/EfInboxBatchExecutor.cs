using EducationSPD.Application.Diagnostics;
using EducationSPD.Application.Inbox;
using EducationSPD.Domain.Inbox;
using EducationSPD.Infrastructure.Configuration;
using EducationSPD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace EducationSPD.Infrastructure.Inbox;

internal sealed class EfInboxBatchExecutor : IInboxBatchExecutor
{
    private readonly AppDbContext dbContext;
    private readonly InboxOptions options;
    private readonly KafkaOptions kafkaOptions;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<EfInboxBatchExecutor> logger;

    public EfInboxBatchExecutor(
        AppDbContext dbContext,
        IOptions<InboxOptions> options,
        IOptions<KafkaOptions> kafkaOptions,
        TimeProvider timeProvider,
        ILogger<EfInboxBatchExecutor> logger)
    {
        this.dbContext = dbContext;
        this.options = options.Value;
        this.kafkaOptions = kafkaOptions.Value;
        this.timeProvider = timeProvider;
        this.logger = logger;
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

        var previousStatuses = messages.ToDictionary(message => message.EventId, message => message.Status);
        try
        {
            await processBatch(messages, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            RecordStatusChanges(messages, previousStatuses);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(
                exception,
                "Техническая ошибка обработки {MessageCount} сообщений inbox; запланирован ограниченный повтор",
                messages.Count);
            await transaction.RollbackAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            await ScheduleTechnicalRetryAsync(
                messages.Select(message => message.EventId).ToArray(),
                cancellationToken);
        }

        return messages.Count;
    }

    private async Task ScheduleTechnicalRetryAsync(
        IReadOnlyCollection<Guid> eventIds,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var messages = await dbContext.InboxMessages
            .Where(message => eventIds.Contains(message.EventId)
                && (message.Status == InboxStatus.Received || message.Status == InboxStatus.PendingCatalog))
            .ToListAsync(cancellationToken);
        var now = timeProvider.GetUtcNow();

        foreach (var message in messages)
        {
            if (message.RetryCount >= kafkaOptions.MaxRetryAttempts - 1)
            {
                if (message.Status == InboxStatus.PendingCatalog)
                {
                    EducationSpdTelemetry.LeavePendingCatalog();
                }

                message.MarkFailed("TechnicalRetryLimitExceeded", now);
                EducationSpdTelemetry.RecordFailedInbox("TechnicalRetryLimitExceeded");
                EducationSpdTelemetry.RecordInboxStatus(nameof(InboxStatus.Failed));
                logger.LogWarning("Сообщение inbox окончательно отклонено после исчерпания технических повторов");
                continue;
            }

            var exponent = Math.Min(message.RetryCount, kafkaOptions.MaxRetryAttempts - 1);
            var delay = TimeSpan.FromMilliseconds(
                kafkaOptions.InitialRetryDelayMilliseconds * Math.Pow(2, exponent));
            message.ScheduleRetry(now.Add(delay));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static void RecordStatusChanges(
        IEnumerable<InboxMessage> messages,
        IReadOnlyDictionary<Guid, InboxStatus> previousStatuses)
    {
        foreach (var message in messages)
        {
            var previousStatus = previousStatuses[message.EventId];
            if (previousStatus != InboxStatus.PendingCatalog && message.Status == InboxStatus.PendingCatalog)
            {
                EducationSpdTelemetry.EnterPendingCatalog();
            }
            else if (previousStatus == InboxStatus.PendingCatalog && message.Status != InboxStatus.PendingCatalog)
            {
                EducationSpdTelemetry.LeavePendingCatalog();
            }

            if (message.Status == InboxStatus.Failed && message.ErrorCode is not null)
            {
                EducationSpdTelemetry.RecordFailedInbox(message.ErrorCode);
            }

            if (previousStatus != message.Status
                && message.Status is InboxStatus.Applied or InboxStatus.PendingCatalog or InboxStatus.Failed)
            {
                EducationSpdTelemetry.RecordInboxStatus(message.Status.ToString());
            }
        }
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
                            CASE
                                WHEN pg_input_is_valid(
                                    earlier_message.payload->>'finalizedAt',
                                    'timestamp with time zone')
                                THEN (earlier_message.payload->>'finalizedAt')::timestamp with time zone
                                ELSE '-infinity'::timestamp with time zone
                            END,
                            CASE
                                WHEN pg_input_is_valid(earlier_message.payload->>'episodeId', 'uuid')
                                THEN (earlier_message.payload->>'episodeId')::uuid
                                ELSE '00000000-0000-0000-0000-000000000000'::uuid
                            END)
                          < (
                            CASE
                                WHEN pg_input_is_valid(
                                    current_message.payload->>'finalizedAt',
                                    'timestamp with time zone')
                                THEN (current_message.payload->>'finalizedAt')::timestamp with time zone
                                ELSE '-infinity'::timestamp with time zone
                            END,
                            CASE
                                WHEN pg_input_is_valid(current_message.payload->>'episodeId', 'uuid')
                                THEN (current_message.payload->>'episodeId')::uuid
                                ELSE '00000000-0000-0000-0000-000000000000'::uuid
                            END))
                ORDER BY
                    CASE
                        WHEN pg_input_is_valid(
                            current_message.payload->>'finalizedAt',
                            'timestamp with time zone')
                        THEN (current_message.payload->>'finalizedAt')::timestamp with time zone
                        ELSE '-infinity'::timestamp with time zone
                    END,
                    CASE
                        WHEN pg_input_is_valid(current_message.payload->>'episodeId', 'uuid')
                        THEN (current_message.payload->>'episodeId')::uuid
                        ELSE '00000000-0000-0000-0000-000000000000'::uuid
                    END,
                    current_message.received_at,
                    current_message.event_id
                LIMIT {options.BatchSize}
                FOR UPDATE OF current_message SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);
    }
}
