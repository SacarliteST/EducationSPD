using System.Text.Json.Serialization;

namespace EducationSPD.Kafka.Contracts.V1;

/// <summary>Событие изменения определения навыка в каталоге учебного модуля.</summary>
/// <param name="SchemaVersion">Версия wire-схемы.</param>
/// <param name="EventId">Уникальный идентификатор доставки события.</param>
/// <param name="ModuleKey">Стабильный ключ учебного модуля.</param>
/// <param name="SkillId">Стабильный идентификатор навыка внутри модуля.</param>
/// <param name="Name">Отображаемое название навыка.</param>
/// <param name="Description">Необязательное описание навыка.</param>
/// <param name="Origin">Происхождение навыка.</param>
/// <param name="IsDeleted">Признак мягкого удаления навыка.</param>
/// <param name="OccurredAt">Момент изменения каталога в UTC.</param>
public sealed record SkillCatalogChanged(
    [property: JsonRequired] int SchemaVersion,
    [property: JsonRequired] Guid EventId,
    [property: JsonRequired] string ModuleKey,
    [property: JsonRequired] string SkillId,
    [property: JsonRequired] string Name,
    string? Description,
    [property: JsonRequired] SkillOrigin Origin,
    [property: JsonRequired] bool IsDeleted,
    [property: JsonRequired] DateTimeOffset OccurredAt) : IKafkaContract;
