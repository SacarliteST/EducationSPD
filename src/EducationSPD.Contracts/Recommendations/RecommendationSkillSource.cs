using System.Text.Json.Serialization;

namespace EducationSPD.Contracts.Recommendations;

/// <summary>Источник оценки навыка кандидата.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<RecommendationSkillSource>))]
public enum RecommendationSkillSource
{
    /// <summary>Точное правило проверки.</summary>
    ValidationRule = 1,

    /// <summary>Педагогический навык-тег.</summary>
    TaskTag = 2,
}
