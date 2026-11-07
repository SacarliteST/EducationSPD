namespace EducationSPD.Application.Episodes;

/// <summary>Результат предметного применения финализированного эпизода.</summary>
public enum LearningEpisodeApplyResult
{
    /// <summary>Эпизод и наблюдения сохранены.</summary>
    Applied = 1,

    /// <summary>Событие уже было применено.</summary>
    Duplicate = 2,

    /// <summary>Идентичность эпизода уже занята другим событием.</summary>
    Conflict = 3,

    /// <summary>Каталог ещё не содержит все навыки эпизода.</summary>
    MissingCatalog = 4
}
