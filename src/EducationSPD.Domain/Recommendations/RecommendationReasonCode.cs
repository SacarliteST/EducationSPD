namespace EducationSPD.Domain.Recommendations;

/// <summary>Стабильный код причины позиции задания в рекомендации.</summary>
public enum RecommendationReasonCode
{
    /// <summary>Задание направлено на навыки с высоким дефицитом.</summary>
    TargetsWeakSkills = 1,

    /// <summary>Сложность задания соответствует текущему уровню.</summary>
    DifficultyMatchesCurrentLevel = 2,

    /// <summary>Студент ещё не финализировал это задание или делал это редко.</summary>
    TaskIsNewForStudent = 3,

    /// <summary>Задание содержит точные правила проверки навыков.</summary>
    ProvidesPreciseAssessment = 4,
}
