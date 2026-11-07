using EducationSPD.Domain.Skills;

namespace EducationSPD.Domain.Episodes;

/// <summary>Неизменяемый итог финализированного учебного эпизода.</summary>
public sealed class LearningEpisode
{
    /// <summary>Максимальное число наблюдений в одном эпизоде.</summary>
    public const int MaxObservationCount = 100;

    private readonly List<SkillObservation> observations = [];

    private LearningEpisode()
    {
    }

    private LearningEpisode(
        Guid id,
        Guid eventId,
        Guid episodeId,
        Guid studentId,
        string moduleKey,
        Guid taskId,
        DateTimeOffset finalizedAt,
        IEnumerable<NewSkillObservation> newObservations)
    {
        Id = id;
        EventId = eventId;
        EpisodeId = episodeId;
        StudentId = studentId;
        ModuleKey = moduleKey;
        TaskId = taskId;
        FinalizedAt = finalizedAt;
        observations.AddRange(newObservations.Select(observation => new SkillObservation(
            observation.Id,
            observation.SkillId,
            observation.Source,
            observation.Outcome)));
    }

    /// <summary>Внутренний идентификатор эпизода.</summary>
    public Guid Id { get; private set; }

    /// <summary>Идентификатор интеграционного события.</summary>
    public Guid EventId { get; private set; }

    /// <summary>Идентификатор эпизода в учебном модуле.</summary>
    public Guid EpisodeId { get; private set; }

    /// <summary>Идентификатор студента платформы.</summary>
    public Guid StudentId { get; private set; }

    /// <summary>Стабильный ключ учебного модуля.</summary>
    public string ModuleKey { get; private set; } = string.Empty;

    /// <summary>Идентификатор задания внутри учебного модуля.</summary>
    public Guid TaskId { get; private set; }

    /// <summary>Момент окончательной финализации в UTC.</summary>
    public DateTimeOffset FinalizedAt { get; private set; }

    /// <summary>Окончательные результаты навыков эпизода.</summary>
    public IReadOnlyCollection<SkillObservation> Observations => observations;

    /// <summary>Создаёт окончательный эпизод вместе со всеми наблюдениями.</summary>
    public static LearningEpisode Create(
        Guid id,
        Guid eventId,
        Guid episodeId,
        Guid studentId,
        string moduleKey,
        Guid taskId,
        DateTimeOffset finalizedAt,
        IReadOnlyCollection<NewSkillObservation> observations)
    {
        RequireGuid(id, nameof(id));
        RequireGuid(eventId, nameof(eventId));
        RequireGuid(episodeId, nameof(episodeId));
        RequireGuid(studentId, nameof(studentId));
        RequireGuid(taskId, nameof(taskId));

        if (string.IsNullOrWhiteSpace(moduleKey)
            || moduleKey.Length > Skill.ModuleKeyMaxLength
            || moduleKey.Any(character =>
                character is not (>= 'a' and <= 'z')
                && character is not (>= '0' and <= '9')
                && character != '-'))
        {
            throw new ArgumentException(
                $"Ключ модуля должен содержать от 1 до {Skill.ModuleKeyMaxLength} символов.",
                nameof(moduleKey));
        }

        if (finalizedAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Время финализации должно быть указано в UTC.", nameof(finalizedAt));
        }

        if (observations is not { Count: >= 1 and <= MaxObservationCount })
        {
            throw new ArgumentException(
                $"Эпизод должен содержать от 1 до {MaxObservationCount} наблюдений.",
                nameof(observations));
        }

        if (observations.Select(observation => observation.SkillId).Distinct().Count() != observations.Count)
        {
            throw new ArgumentException("Один навык не может повторяться внутри эпизода.", nameof(observations));
        }

        if (observations.Select(observation => observation.Id).Distinct().Count() != observations.Count)
        {
            throw new ArgumentException("Идентификатор наблюдения не может повторяться внутри эпизода.", nameof(observations));
        }

        return new LearningEpisode(
            id,
            eventId,
            episodeId,
            studentId,
            moduleKey,
            taskId,
            finalizedAt,
            observations);
    }

    private static void RequireGuid(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Идентификатор не может быть пустым UUID.", parameterName);
        }
    }
}
