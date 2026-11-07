namespace EducationSPD.Domain.Episodes;

/// <summary>Бинарный итог наблюдения навыка.</summary>
public enum SkillObservationOutcome : short
{
    /// <summary>Навык подтверждён итогом эпизода.</summary>
    Succeeded = 1,

    /// <summary>Навык не подтверждён итогом эпизода.</summary>
    Failed = 2
}
