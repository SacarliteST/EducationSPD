namespace EducationSPD.Domain.Episodes;

/// <summary>Неизменяемый бинарный результат одного навыка в учебном эпизоде.</summary>
public sealed class SkillObservation
{
    private SkillObservation()
    {
    }

    internal SkillObservation(
        Guid id,
        Guid skillId,
        SkillObservationSource source,
        SkillObservationOutcome outcome)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Идентификатор наблюдения не может быть пустым UUID.", nameof(id));
        }

        if (skillId == Guid.Empty)
        {
            throw new ArgumentException("Идентификатор навыка не может быть пустым UUID.", nameof(skillId));
        }

        if (!Enum.IsDefined(source))
        {
            throw new ArgumentOutOfRangeException(nameof(source), "Источник наблюдения не поддерживается.");
        }

        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome), "Результат наблюдения не поддерживается.");
        }

        Id = id;
        SkillId = skillId;
        Source = source;
        Outcome = outcome;
    }

    /// <summary>Внутренний идентификатор наблюдения.</summary>
    public Guid Id { get; private set; }

    /// <summary>Внутренний идентификатор учебного эпизода.</summary>
    public Guid LearningEpisodeId { get; private set; }

    /// <summary>Внутренний идентификатор определения навыка из каталога.</summary>
    public Guid SkillId { get; private set; }

    /// <summary>Источник результата навыка.</summary>
    public SkillObservationSource Source { get; private set; }

    /// <summary>Бинарный итог навыка.</summary>
    public SkillObservationOutcome Outcome { get; private set; }
}
