using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace EducationSPD.Application.Diagnostics;

/// <summary>Единые источники трассировки и метрик EducationSPD.</summary>
public static class EducationSpdTelemetry
{
    /// <summary>Имя источника телеметрии.</summary>
    public const string SourceName = "EducationSPD";

    /// <summary>Источник распределённых трассировок.</summary>
    public static readonly ActivitySource ActivitySource = new(SourceName);

    private static readonly Meter Meter = new(SourceName);
    private static readonly Counter<long> KafkaMessages = Meter.CreateCounter<long>(
        "educationspd_kafka_messages_total");
    private static readonly UpDownCounter<long> PendingCatalog = Meter.CreateUpDownCounter<long>(
        "educationspd_inbox_pending_catalog");
    private static readonly Counter<long> FailedInbox = Meter.CreateCounter<long>(
        "educationspd_inbox_failed_total");
    private static readonly Counter<long> InboxStatuses = Meter.CreateCounter<long>(
        "educationspd_inbox_messages_total");
    private static readonly Histogram<double> EpisodeDuration = Meter.CreateHistogram<double>(
        "educationspd_episode_apply_duration_seconds",
        "s");
    private static readonly Counter<long> ProjectionRecalculations = Meter.CreateCounter<long>(
        "educationspd_projection_recalculations_total");
    private static readonly Counter<long> RecommendationRequests = Meter.CreateCounter<long>(
        "educationspd_recommendation_requests_total");
    private static readonly Histogram<double> RecommendationDuration = Meter.CreateHistogram<double>(
        "educationspd_recommendation_duration_seconds",
        "s");
    private static readonly Histogram<long> RecommendationCandidates = Meter.CreateHistogram<long>(
        "educationspd_recommendation_candidates");
    private static readonly Counter<long> ModelRecalculationProgress = Meter.CreateCounter<long>(
        "educationspd_model_recalculation_progress");

    /// <summary>Учитывает результат чтения Kafka-сообщения.</summary>
    public static void RecordKafkaMessage(string topic, string status) =>
        KafkaMessages.Add(1, new KeyValuePair<string, object?>("topic", topic),
            new KeyValuePair<string, object?>("status", status));

    /// <summary>Учитывает новое сообщение, ожидающее каталог.</summary>
    public static void EnterPendingCatalog() => PendingCatalog.Add(1);

    /// <summary>Учитывает выход сообщения из ожидания каталога.</summary>
    public static void LeavePendingCatalog() => PendingCatalog.Add(-1);

    /// <summary>Учитывает окончательно отклонённое сообщение.</summary>
    public static void RecordFailedInbox(string errorCode) =>
        FailedInbox.Add(1, new KeyValuePair<string, object?>("error_code", errorCode));

    /// <summary>Учитывает переход сообщения в прикладное состояние.</summary>
    public static void RecordInboxStatus(string status) =>
        InboxStatuses.Add(1, new KeyValuePair<string, object?>("status", status));

    /// <summary>Записывает длительность применения эпизода.</summary>
    public static void RecordEpisodeDuration(TimeSpan duration) =>
        EpisodeDuration.Record(duration.TotalSeconds);

    /// <summary>Учитывает результат пересчёта проекции.</summary>
    public static void RecordProjectionRecalculation(string status) =>
        ProjectionRecalculations.Add(1, new KeyValuePair<string, object?>("status", status));

    /// <summary>Записывает результат HTTP-ранжирования.</summary>
    public static void RecordRecommendation(string status, TimeSpan duration, int candidates)
    {
        RecommendationRequests.Add(1, new KeyValuePair<string, object?>("status", status));
        RecommendationDuration.Record(duration.TotalSeconds);
        RecommendationCandidates.Record(candidates);
    }

    /// <summary>Учитывает число пересчитанных пар студент-модуль.</summary>
    public static void RecordModelRecalculationProgress(int pairs) =>
        ModelRecalculationProgress.Add(pairs);
}
