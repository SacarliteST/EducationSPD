namespace EducationSPD.Application.Inbox;

/// <summary>Надёжно сохраняет транспортные сообщения до подтверждения Kafka offset.</summary>
public interface IInboxStore
{
    /// <summary>Сохраняет сообщение или определяет результат повторной доставки.</summary>
    Task<InboxWriteResult> StoreAsync(
        InboxMessageInput message,
        CancellationToken cancellationToken);
}
