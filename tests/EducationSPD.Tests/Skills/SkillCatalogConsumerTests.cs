using EducationSPD.Domain.Inbox;
using EducationSPD.Kafka.Contracts;
using EducationSPD.Kafka.Contracts.V1;
using EducationSPD.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Tests.Skills;

public sealed class SkillCatalogConsumerTests(KafkaCatalogFixture fixture)
    : IClassFixture<KafkaCatalogFixture>
{
    [Fact]
    public async Task Consumer_PersistsThenAppliesCatalogIdempotently()
    {
        await fixture.StartConsumerAsync();
        var firstEvent = CreateEvent("JOIN", false, DateTimeOffset.UtcNow.AddMinutes(-2));
        var firstPayload = KafkaContractJson.Serialize(firstEvent);

        await fixture.PublishAsync("sql:join", firstPayload);
        await WaitUntilAsync(async dbContext =>
            await dbContext.InboxMessages.AnyAsync(message => message.EventId == firstEvent.EventId));

        await using (var storedContext = fixture.CreateDbContext())
        {
            var stored = await storedContext.InboxMessages
                .AsNoTracking()
                .SingleAsync(message => message.EventId == firstEvent.EventId);
            Assert.Equal(InboxStatus.Received, stored.Status);
            Assert.False(await storedContext.Skills.AnyAsync());
        }

        await fixture.StartProcessorAsync();
        await WaitForInboxStatusAsync(firstEvent.EventId, InboxStatus.Applied);

        await fixture.PublishAsync("sql:join", firstPayload);
        var deletedEvent = CreateEvent("JOIN", true, firstEvent.OccurredAt.AddMinutes(1));
        await fixture.PublishAsync("sql:join", KafkaContractJson.Serialize(deletedEvent));
        await WaitForInboxStatusAsync(deletedEvent.EventId, InboxStatus.Applied);

        var staleEvent = CreateEvent("Устаревшее имя", false, firstEvent.OccurredAt.AddSeconds(-1));
        await fixture.PublishAsync("sql:join", KafkaContractJson.Serialize(staleEvent));
        await WaitForInboxStatusAsync(staleEvent.EventId, InboxStatus.Applied);

        await using var verificationContext = fixture.CreateDbContext();
        var skill = await verificationContext.Skills.AsNoTracking().SingleAsync();
        Assert.True(skill.IsDeleted);
        Assert.Equal("JOIN", skill.Name);
        Assert.Equal(deletedEvent.EventId, skill.SourceEventId);
        Assert.Equal(1, await verificationContext.InboxMessages.CountAsync(
            message => message.EventId == firstEvent.EventId));
        Assert.Equal(1, await verificationContext.Skills.CountAsync());

        var eventId = Guid.NewGuid();
        var payload = $$"""
            {
              "schemaVersion": 2,
              "eventId": "{{eventId}}",
              "moduleKey": "sql",
              "skillId": "join",
              "name": "JOIN",
              "origin": "System",
              "isDeleted": false,
              "occurredAt": "{{DateTimeOffset.UtcNow:O}}"
            }
            """;

        await fixture.PublishAsync("sql:join", payload);
        await WaitForInboxStatusAsync(eventId, InboxStatus.Failed);

        await using var failedContext = fixture.CreateDbContext();
        var inboxMessage = await failedContext.InboxMessages.AsNoTracking()
            .SingleAsync(message => message.EventId == eventId);
        Assert.Equal("UnsupportedSchemaVersion", inboxMessage.ErrorCode);
        Assert.DoesNotContain("JOIN", inboxMessage.ErrorCode, StringComparison.Ordinal);
    }

    private static SkillCatalogChanged CreateEvent(
        string name,
        bool isDeleted,
        DateTimeOffset occurredAt)
    {
        return new SkillCatalogChanged(
            KafkaSchemaVersions.Version1,
            Guid.NewGuid(),
            "sql",
            "join",
            name,
            null,
            SkillOrigin.System,
            isDeleted,
            occurredAt);
    }

    private async Task WaitForInboxStatusAsync(Guid eventId, InboxStatus expectedStatus)
    {
        await WaitUntilAsync(async dbContext =>
            await dbContext.InboxMessages.AnyAsync(
                message => message.EventId == eventId && message.Status == expectedStatus));
    }

    private async Task WaitUntilAsync(
        Func<EducationSPD.Infrastructure.Persistence.AppDbContext, Task<bool>> condition)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(15);
        while (DateTimeOffset.UtcNow < deadline)
        {
            await using var dbContext = fixture.CreateDbContext();
            if (await condition(dbContext))
            {
                return;
            }

            await Task.Delay(100);
        }

        Assert.Fail("Ожидаемое состояние Kafka-сценария не достигнуто за 15 секунд.");
    }
}
