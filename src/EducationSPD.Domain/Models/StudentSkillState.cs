using EducationSPD.Domain.Skills;

namespace EducationSPD.Domain.Models;

/// <summary>Текущая BKT-проекция одного навыка студента.</summary>
public sealed class StudentSkillState
{
    private StudentSkillState()
    {
    }

    private StudentSkillState(Guid studentId, string moduleKey, Guid skillId, int modelVersion)
    {
        if (studentId == Guid.Empty)
        {
            throw new ArgumentException("Идентификатор студента не может быть пустым.", nameof(studentId));
        }

        if (string.IsNullOrWhiteSpace(moduleKey) || moduleKey.Length > Skill.ModuleKeyMaxLength)
        {
            throw new ArgumentException("Ключ модуля заполнен некорректно.", nameof(moduleKey));
        }

        if (skillId == Guid.Empty)
        {
            throw new ArgumentException("Идентификатор навыка не может быть пустым.", nameof(skillId));
        }

        if (modelVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(modelVersion));
        }

        StudentId = studentId;
        ModuleKey = moduleKey;
        SkillId = skillId;
        ModelVersion = modelVersion;
    }

    /// <summary>Идентификатор студента.</summary>
    public Guid StudentId { get; private set; }

    /// <summary>Ключ учебного модуля.</summary>
    public string ModuleKey { get; private set; } = string.Empty;

    /// <summary>Идентификатор навыка.</summary>
    public Guid SkillId { get; private set; }

    /// <summary>Версия модели, по которой построена проекция.</summary>
    public int ModelVersion { get; private set; }

    /// <summary>Текущая вероятность освоения.</summary>
    public decimal MasteryProbability { get; private set; }

    /// <summary>Количество учтённых наблюдений.</summary>
    public int ObservationsCount { get; private set; }

    /// <summary>Педагогическое время последнего наблюдения.</summary>
    public DateTimeOffset? LastObservedAt { get; private set; }

    /// <summary>Время последнего построения проекции.</summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Создаёт пустую проекцию для заполнения из истории.</summary>
    public static StudentSkillState Create(
        Guid studentId,
        string moduleKey,
        Guid skillId,
        int modelVersion) =>
        new(studentId, moduleKey, skillId, modelVersion);

    /// <summary>Заменяет вычисляемое состояние результатом полного пересчёта истории.</summary>
    public void Rebuild(
        decimal masteryProbability,
        int observationsCount,
        DateTimeOffset lastObservedAt,
        DateTimeOffset updatedAt)
    {
        if (masteryProbability is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(masteryProbability));
        }

        if (observationsCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(observationsCount));
        }

        if (lastObservedAt.Offset != TimeSpan.Zero || updatedAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Время проекции должно быть указано в UTC.");
        }

        MasteryProbability = masteryProbability;
        ObservationsCount = observationsCount;
        LastObservedAt = lastObservedAt;
        UpdatedAt = updatedAt;
    }
}
