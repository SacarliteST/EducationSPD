namespace EducationSPD.Infrastructure.Configuration;

/// <summary>Параметры фонового полного пересчёта модели.</summary>
public sealed class RecalculationOptions
{
    /// <summary>Имя секции конфигурации.</summary>
    public const string SectionName = "Recalculation";

    /// <summary>Количество пар студент-модуль в одной транзакции.</summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>Интервал проверки новых черновых версий.</summary>
    public int PollIntervalSeconds { get; set; } = 5;

    internal static bool IsValid(RecalculationOptions options) =>
        options.BatchSize is >= 1 and <= 1000
        && options.PollIntervalSeconds is >= 1 and <= 300;
}
