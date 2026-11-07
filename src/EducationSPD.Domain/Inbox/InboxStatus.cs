namespace EducationSPD.Domain.Inbox;

/// <summary>Состояние обработки сообщения durable inbox.</summary>
public enum InboxStatus : short
{
    /// <summary>Сообщение сохранено и готово к предметной обработке.</summary>
    Received = 1,

    /// <summary>Обработка ожидает появления определения навыка.</summary>
    PendingCatalog = 2,

    /// <summary>Сообщение успешно применено.</summary>
    Applied = 3,

    /// <summary>Сообщение окончательно отклонено.</summary>
    Failed = 4
}
