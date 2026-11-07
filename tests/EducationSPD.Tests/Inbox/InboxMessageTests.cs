using EducationSPD.Domain.Inbox;

namespace EducationSPD.Tests.Inbox;

public sealed class InboxMessageTests
{
    private static readonly DateTimeOffset ReceivedAt = new(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ReceivedMessage_CanWaitForCatalogAndThenBeApplied()
    {
        var message = CreateMessage();
        var nextRetryAt = ReceivedAt.AddMinutes(1);

        message.WaitForCatalog(nextRetryAt);

        Assert.Equal(InboxStatus.PendingCatalog, message.Status);
        Assert.Equal(nextRetryAt, message.NextRetryAt);

        var processedAt = ReceivedAt.AddMinutes(2);
        message.MarkApplied(processedAt);

        Assert.Equal(InboxStatus.Applied, message.Status);
        Assert.Equal(processedAt, message.ProcessedAt);
        Assert.Null(message.NextRetryAt);
        Assert.Throws<InvalidOperationException>(() => message.MarkFailed("LateFailure", processedAt));
    }

    [Fact]
    public void TechnicalRetry_IncrementsMetadataWithoutChangingStatus()
    {
        var message = CreateMessage();
        var nextRetryAt = ReceivedAt.AddMinutes(1);

        message.ScheduleRetry(nextRetryAt);

        Assert.Equal(InboxStatus.Received, message.Status);
        Assert.Equal(1, message.RetryCount);
        Assert.Equal(nextRetryAt, message.NextRetryAt);
    }

    [Fact]
    public void FailedMessage_CanBeRestartedByOperationalCommand()
    {
        var message = CreateMessage();
        var failedAt = ReceivedAt.AddMinutes(1);
        message.MarkFailed("ContractInvalid", failedAt);

        message.Restart(failedAt.AddMinutes(1));

        Assert.Equal(InboxStatus.Received, message.Status);
        Assert.Null(message.ErrorCode);
        Assert.Null(message.ProcessedAt);
        Assert.Equal(0, message.RetryCount);
    }

    private static InboxMessage CreateMessage()
    {
        return InboxMessage.Receive(
            Guid.NewGuid(),
            InboxMessageType.SkillCatalogChanged,
            1,
            "sql:join",
            "{}",
            new string('a', InboxMessage.PayloadHashLength),
            ReceivedAt);
    }
}
