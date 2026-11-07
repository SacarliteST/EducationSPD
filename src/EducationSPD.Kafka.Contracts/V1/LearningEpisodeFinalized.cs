using System.Text.Json.Serialization;

namespace EducationSPD.Kafka.Contracts.V1;

/// <summary>Событие окончательной финализации одного учебного эпизода.</summary>
/// <param name="SchemaVersion">Версия wire-схемы.</param>
/// <param name="EventId">Уникальный идентификатор доставки события.</param>
/// <param name="EpisodeId">Уникальный идентификатор учебного эпизода.</param>
/// <param name="StudentId">Идентификатор студента платформы.</param>
/// <param name="ModuleKey">Стабильный ключ учебного модуля.</param>
/// <param name="TaskId">Идентификатор задания в учебном модуле.</param>
/// <param name="FinalizedAt">Момент финализации эпизода в UTC.</param>
/// <param name="SkillResults">Окончательные результаты навыков эпизода.</param>
public sealed record LearningEpisodeFinalized(
    [property: JsonRequired] int SchemaVersion,
    [property: JsonRequired] Guid EventId,
    [property: JsonRequired] Guid EpisodeId,
    [property: JsonRequired] Guid StudentId,
    [property: JsonRequired] string ModuleKey,
    [property: JsonRequired] Guid TaskId,
    [property: JsonRequired] DateTimeOffset FinalizedAt,
    [property: JsonRequired] IReadOnlyList<SkillResult> SkillResults) : IKafkaContract;
