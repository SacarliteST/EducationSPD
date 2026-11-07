using System.Text.Json.Serialization;

namespace EducationSPD.Kafka.Contracts.V1;

/// <summary>Итог одного навыка в финализированном учебном эпизоде.</summary>
/// <param name="SkillId">Стабильный идентификатор навыка внутри модуля.</param>
/// <param name="Source">Источник оценки навыка.</param>
/// <param name="Outcome">Бинарный итог навыка.</param>
public sealed record SkillResult(
    [property: JsonRequired] string SkillId,
    [property: JsonRequired] SkillResultSource Source,
    [property: JsonRequired] SkillOutcome Outcome);
