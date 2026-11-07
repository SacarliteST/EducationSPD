using System.Text.Json;
using EducationSPD.Application.Inbox;
using EducationSPD.Domain.Inbox;
using EducationSPD.Infrastructure.Configuration;
using EducationSPD.Infrastructure.Inbox;
using EducationSPD.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EducationSPD.Tests.Inbox;

public sealed class InboxPersistenceTests(PostgresDatabaseFixture database)
    : IClassFixture<PostgresDatabaseFixture>
{
    [Fact]
    public async Task RepeatedEventId_DistinguishesDuplicateAndPayloadConflict()
    {
        await database.ResetInboxAsync();
        var eventId = Guid.NewGuid();
        const string originalPayload = "{\"schemaVersion\":1,\"name\":\"JOIN\"}";

        var firstResult = await StoreAsync(eventId, originalPayload);
        var duplicateResult = await StoreAsync(eventId, originalPayload);
        var conflictResult = await StoreAsync(
            eventId,
            "{\"schemaVersion\":1,\"name\":\"Другой навык\"}");

        Assert.Equal(InboxWriteResult.Stored, firstResult);
        Assert.Equal(InboxWriteResult.Duplicate, duplicateResult);
        Assert.Equal(InboxWriteResult.Conflict, conflictResult);

        await using var verificationContext = database.CreateDbContext();
        var stored = await verificationContext.InboxMessages.AsNoTracking().SingleAsync();
        Assert.Equal(InboxStatus.Received, stored.Status);
        Assert.Equal(InboxMessage.PayloadHashLength, stored.PayloadHash.Length);
        using var payload = JsonDocument.Parse(stored.Payload);
        Assert.Equal("JOIN", payload.RootElement.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ParallelBatchExecutors_LockDifferentMessages()
    {
        await database.ResetInboxAsync();
        for (var index = 0; index < 4; index++)
        {
            await StoreAsync(
                Guid.NewGuid(),
                $"{{\"schemaVersion\":1,\"sequence\":{index}}}");
        }

        await using var firstContext = database.CreateDbContext();
        await using var secondContext = database.CreateDbContext();
        var firstExecutor = CreateExecutor(firstContext, 2);
        var secondExecutor = CreateExecutor(secondContext, 2);
        var firstBatchLocked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirstBatch = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstIds = new List<Guid>();
        var secondIds = new List<Guid>();

        var firstExecution = firstExecutor.ExecuteReadyBatchAsync(
            async (messages, _) =>
            {
                firstIds.AddRange(messages.Select(message => message.EventId));
                firstBatchLocked.SetResult();
                await releaseFirstBatch.Task;
                MarkApplied(messages);
            },
            CancellationToken.None);

        await firstBatchLocked.Task;
        try
        {
            var secondCount = await secondExecutor.ExecuteReadyBatchAsync(
                (messages, _) =>
                {
                    secondIds.AddRange(messages.Select(message => message.EventId));
                    MarkApplied(messages);
                    return Task.CompletedTask;
                },
                CancellationToken.None);

            Assert.Equal(2, secondCount);
        }
        finally
        {
            releaseFirstBatch.SetResult();
        }

        Assert.Equal(2, await firstExecution);
        Assert.Empty(firstIds.Intersect(secondIds));

        await using var verificationContext = database.CreateDbContext();
        Assert.Equal(
            4,
            await verificationContext.InboxMessages.CountAsync(
                message => message.Status == InboxStatus.Applied));
    }

    private async Task<InboxWriteResult> StoreAsync(Guid eventId, string payload)
    {
        await using var dbContext = database.CreateDbContext();
        var store = new EfInboxStore(dbContext, TimeProvider.System);
        return await store.StoreAsync(
            new InboxMessageInput(
                eventId,
                InboxMessageType.SkillCatalogChanged,
                1,
                $"sql:{eventId:N}",
                payload),
            CancellationToken.None);
    }

    private static EfInboxBatchExecutor CreateExecutor(
        EducationSPD.Infrastructure.Persistence.AppDbContext dbContext,
        int batchSize)
    {
        var options = Options.Create(new InboxOptions
        {
            BatchSize = batchSize,
            PendingCatalogRetryIntervalSeconds = 30
        });

        return new EfInboxBatchExecutor(dbContext, options, TimeProvider.System);
    }

    private static void MarkApplied(IReadOnlyList<InboxMessage> messages)
    {
        var processedAt = DateTimeOffset.UtcNow;
        foreach (var message in messages)
        {
            message.MarkApplied(processedAt);
        }
    }
}
