using System.Text.Json.Serialization;

namespace EducationSPD.Contracts.Recommendations;

/// <summary>Код причины позиции задания.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<RecommendationReasonCodeResponse>))]
public enum RecommendationReasonCodeResponse
{
    /// <summary>Задание направлено на навыки с высоким дефицитом.</summary>
    TargetsWeakSkills = 1,

    /// <summary>Сложность соответствует текущему уровню.</summary>
    DifficultyMatchesCurrentLevel = 2,

    /// <summary>Задание ещё не выполнялось или выполнялось редко.</summary>
    TaskIsNewForStudent = 3,

    /// <summary>Задание содержит точные правила проверки.</summary>
    ProvidesPreciseAssessment = 4,
}
