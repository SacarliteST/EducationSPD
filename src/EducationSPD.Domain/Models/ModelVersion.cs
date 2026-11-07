namespace EducationSPD.Domain.Models;

/// <summary>Версионируемая конфигурация математической модели.</summary>
public sealed class ModelVersion
{
    private decimal[] ahpMatrix = [];
    private decimal[] ahpWeights = [];

    private ModelVersion()
    {
    }

    private ModelVersion(Guid id, int version, ModelParameters parameters, ModelVersionStatus status, DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Идентификатор версии модели не может быть пустым.", nameof(id));
        }

        if (version <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(version), "Номер версии должен быть положительным.");
        }

        if (createdAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Время создания должно быть указано в UTC.", nameof(createdAt));
        }

        Id = id;
        Version = version;
        InitialMasteryProbability = parameters.InitialMasteryProbability;
        LearnProbability = parameters.LearnProbability;
        ValidationRuleGuessProbability = parameters.ValidationRuleGuessProbability;
        ValidationRuleSlipProbability = parameters.ValidationRuleSlipProbability;
        TaskTagGuessProbability = parameters.TaskTagGuessProbability;
        TaskTagSlipProbability = parameters.TaskTagSlipProbability;
        ahpMatrix = parameters.AhpMatrix.ToArray();
        ahpWeights = parameters.AhpWeights.ToArray();
        Status = status;
        CreatedAt = createdAt;
        ActivatedAt = status == ModelVersionStatus.Active ? createdAt : null;
    }

    /// <summary>Идентификатор записи.</summary>
    public Guid Id { get; private set; }

    /// <summary>Монотонно возрастающий номер версии.</summary>
    public int Version { get; private set; }

    /// <summary>Начальная вероятность освоения.</summary>
    public decimal InitialMasteryProbability { get; private set; }

    /// <summary>Вероятность обучения.</summary>
    public decimal LearnProbability { get; private set; }

    /// <summary>Вероятность угадывания для точного правила.</summary>
    public decimal ValidationRuleGuessProbability { get; private set; }

    /// <summary>Вероятность ошибки для точного правила.</summary>
    public decimal ValidationRuleSlipProbability { get; private set; }

    /// <summary>Вероятность угадывания для навыка-тега.</summary>
    public decimal TaskTagGuessProbability { get; private set; }

    /// <summary>Вероятность ошибки для навыка-тега.</summary>
    public decimal TaskTagSlipProbability { get; private set; }

    /// <summary>Матрица попарных сравнений в построчном порядке.</summary>
    public IReadOnlyList<decimal> AhpMatrix => ahpMatrix;

    /// <summary>Вычисленные веса критериев.</summary>
    public IReadOnlyList<decimal> AhpWeights => ahpWeights;

    /// <summary>Состояние жизненного цикла.</summary>
    public ModelVersionStatus Status { get; private set; }

    /// <summary>Время создания версии.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Время начала полного пересчёта.</summary>
    public DateTimeOffset? RecalculationStartedAt { get; private set; }

    /// <summary>Время активации целостной версии.</summary>
    public DateTimeOffset? ActivatedAt { get; private set; }

    /// <summary>Время переноса версии в архив.</summary>
    public DateTimeOffset? ArchivedAt { get; private set; }

    /// <summary>Создаёт новую черновую версию без изменения существующих версий.</summary>
    public static ModelVersion CreateDraft(Guid id, int version, ModelParameters parameters, DateTimeOffset createdAt) =>
        new(id, version, parameters, ModelVersionStatus.Draft, createdAt);

    /// <summary>Возвращает проверенный набор параметров сохранённой версии.</summary>
    public ModelParameters GetParameters() =>
        new(
            InitialMasteryProbability,
            LearnProbability,
            ValidationRuleGuessProbability,
            ValidationRuleSlipProbability,
            TaskTagGuessProbability,
            TaskTagSlipProbability,
            ahpMatrix);

    /// <summary>Переводит черновик в состояние полного пересчёта.</summary>
    public void StartRecalculation(DateTimeOffset startedAt)
    {
        EnsureTransition(ModelVersionStatus.Draft, startedAt);
        Status = ModelVersionStatus.Recalculating;
        RecalculationStartedAt = startedAt;
    }

    /// <summary>Помечает полностью пересчитанную версию активной.</summary>
    public void Activate(DateTimeOffset activatedAt)
    {
        EnsureTransition(ModelVersionStatus.Recalculating, activatedAt);
        Status = ModelVersionStatus.Active;
        ActivatedAt = activatedAt;
    }

    /// <summary>Архивирует ранее активную версию.</summary>
    public void Archive(DateTimeOffset archivedAt)
    {
        EnsureTransition(ModelVersionStatus.Active, archivedAt);
        Status = ModelVersionStatus.Archived;
        ArchivedAt = archivedAt;
    }

    private void EnsureTransition(ModelVersionStatus requiredStatus, DateTimeOffset occurredAt)
    {
        if (Status != requiredStatus)
        {
            throw new InvalidOperationException($"Переход недоступен для состояния {Status}.");
        }

        if (occurredAt.Offset != TimeSpan.Zero || occurredAt < CreatedAt)
        {
            throw new ArgumentException("Время перехода должно быть указано в UTC и не предшествовать созданию версии.", nameof(occurredAt));
        }
    }
}
