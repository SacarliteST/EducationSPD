namespace EducationSPD.Domain.Episodes;

/// <summary>Источник бинарного результата навыка.</summary>
public enum SkillObservationSource : short
{
    /// <summary>Точное правило проверки учебного модуля.</summary>
    ValidationRule = 1,

    /// <summary>Педагогический навык-тег задания.</summary>
    TaskTag = 2
}
