namespace EducationSPD.Kafka.Contracts.V1;

/// <summary>Источник итогового результата навыка.</summary>
public enum SkillResultSource
{
    /// <summary>Результат точного правила проверки учебного модуля.</summary>
    ValidationRule = 1,

    /// <summary>Результат педагогического навыка-тега задания.</summary>
    TaskTag = 2
}
