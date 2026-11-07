namespace EducationSPD.Kafka.Contracts.V1;

/// <summary>Бинарный итог наблюдения навыка.</summary>
public enum SkillOutcome
{
    /// <summary>Навык подтверждён результатом эпизода.</summary>
    Succeeded = 1,

    /// <summary>Навык не подтверждён результатом эпизода.</summary>
    Failed = 2
}
