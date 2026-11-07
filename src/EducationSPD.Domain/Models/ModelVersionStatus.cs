namespace EducationSPD.Domain.Models;

/// <summary>Состояние версии математической модели.</summary>
public enum ModelVersionStatus : short
{
    /// <summary>Параметры подготовлены, но ещё не применяются.</summary>
    Draft = 1,

    /// <summary>Для версии строятся новые проекции знаний.</summary>
    Recalculating = 2,

    /// <summary>Целостная версия используется для расчётов.</summary>
    Active = 3,

    /// <summary>Версия сохранена только как история.</summary>
    Archived = 4,
}
