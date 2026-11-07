namespace EducationSPD.Infrastructure.Configuration;

/// <summary>
/// Настройки Kafka-интеграции EducationSPD.
/// </summary>
public sealed class KafkaOptions
{
    /// <summary>Имя секции конфигурации.</summary>
    public const string SectionName = "Kafka";

    /// <summary>Адреса Kafka brokers.</summary>
    public string BootstrapServers { get; set; } = string.Empty;

    /// <summary>Consumer group EducationSPD.</summary>
    public string ConsumerGroup { get; set; } = string.Empty;

    /// <summary>Топик каталога навыков.</summary>
    public string SkillCatalogTopic { get; set; } = string.Empty;

    /// <summary>Топик финализированных учебных эпизодов.</summary>
    public string EpisodeFinalizedTopic { get; set; } = string.Empty;

    /// <summary>Максимальное количество технических повторов.</summary>
    public int MaxRetryAttempts { get; set; }

    /// <summary>Начальная задержка повтора в миллисекундах.</summary>
    public int InitialRetryDelayMilliseconds { get; set; }

    internal static bool IsValid(KafkaOptions options) =>
        !string.IsNullOrWhiteSpace(options.BootstrapServers)
        && !string.IsNullOrWhiteSpace(options.ConsumerGroup)
        && !string.IsNullOrWhiteSpace(options.SkillCatalogTopic)
        && !string.IsNullOrWhiteSpace(options.EpisodeFinalizedTopic)
        && options.MaxRetryAttempts > 0
        && options.InitialRetryDelayMilliseconds > 0;
}
