namespace EducationSPD.Infrastructure.Configuration;

/// <summary>
/// Настройки фоновой обработки inbox.
/// </summary>
public sealed class InboxOptions
{
    /// <summary>Имя секции конфигурации.</summary>
    public const string SectionName = "Inbox";

    /// <summary>Максимальный размер одного пакета фоновой обработки.</summary>
    public int BatchSize { get; set; }

    /// <summary>Период повторной проверки ожидающих каталог сообщений, в секундах.</summary>
    public int PendingCatalogRetryIntervalSeconds { get; set; }

    internal static bool IsValid(InboxOptions options) =>
        options.BatchSize is > 0 and <= 1000
        && options.PendingCatalogRetryIntervalSeconds > 0;
}
