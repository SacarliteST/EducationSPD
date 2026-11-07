namespace EducationSPD.Application.Inbox;

/// <summary>Результат сохранения транспортного сообщения.</summary>
public enum InboxWriteResult
{
    /// <summary>Новое сообщение сохранено.</summary>
    Stored = 1,

    /// <summary>Повторная доставка содержит тот же payload.</summary>
    Duplicate = 2,

    /// <summary>Тот же EventId повторно использован с другим payload.</summary>
    Conflict = 3
}
