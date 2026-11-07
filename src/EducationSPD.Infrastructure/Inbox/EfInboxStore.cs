using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EducationSPD.Application.Inbox;
using EducationSPD.Domain.Inbox;
using EducationSPD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Infrastructure.Inbox;

internal sealed class EfInboxStore : IInboxStore
{
    private readonly AppDbContext dbContext;
    private readonly TimeProvider timeProvider;

    public EfInboxStore(AppDbContext dbContext, TimeProvider timeProvider)
    {
        this.dbContext = dbContext;
        this.timeProvider = timeProvider;
    }

    public async Task<InboxWriteResult> StoreAsync(
        InboxMessageInput message,
        CancellationToken cancellationToken)
    {
        EnsureJsonObject(message.Payload);

        var payloadHash = ComputeHash(message.Payload);
        var inboxMessage = InboxMessage.Receive(
            message.EventId,
            message.MessageType,
            message.SchemaVersion,
            message.MessageKey,
            message.Payload,
            payloadHash,
            timeProvider.GetUtcNow());

        var messageType = (short)inboxMessage.MessageType;
        var status = (short)inboxMessage.Status;
        var insertedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "InboxMessages"
                (event_id, message_type, schema_version, message_key, payload, payload_hash,
                 status, error_code, received_at, processed_at, retry_count, next_retry_at)
            VALUES
                ({inboxMessage.EventId}, {messageType}, {inboxMessage.SchemaVersion},
                 {inboxMessage.MessageKey}, {inboxMessage.Payload}::jsonb, {inboxMessage.PayloadHash},
                 {status}, NULL, {inboxMessage.ReceivedAt}, NULL, {inboxMessage.RetryCount}, NULL)
            ON CONFLICT (event_id) DO NOTHING;
            """, cancellationToken);

        if (insertedRows == 1)
        {
            return InboxWriteResult.Stored;
        }

        var storedHash = await dbContext.InboxMessages
            .AsNoTracking()
            .Where(stored => stored.EventId == message.EventId)
            .Select(stored => stored.PayloadHash)
            .SingleAsync(cancellationToken);

        return string.Equals(storedHash, payloadHash, StringComparison.Ordinal)
            ? InboxWriteResult.Duplicate
            : InboxWriteResult.Conflict;
    }

    private static string ComputeHash(string payload)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexStringLower(hash);
    }

    private static void EnsureJsonObject(string payload)
    {
        using var document = JsonDocument.Parse(payload);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("Kafka payload должен содержать JSON-объект.");
        }
    }
}
