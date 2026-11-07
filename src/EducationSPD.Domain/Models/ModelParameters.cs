namespace EducationSPD.Domain.Models;

/// <summary>Неизменяемый набор параметров одной версии математической модели.</summary>
public sealed class ModelParameters
{
    /// <summary>Размер матрицы попарных сравнений МАИ.</summary>
    public const int AhpMatrixSize = 4;

    /// <summary>Создаёт и проверяет параметры модели.</summary>
    public ModelParameters(
        decimal initialMasteryProbability,
        decimal learnProbability,
        decimal validationRuleGuessProbability,
        decimal validationRuleSlipProbability,
        decimal taskTagGuessProbability,
        decimal taskTagSlipProbability,
        IReadOnlyList<decimal> ahpMatrix)
    {
        ValidateProbability(initialMasteryProbability, nameof(initialMasteryProbability));
        ValidateProbability(learnProbability, nameof(learnProbability));
        ValidateObservation(
            validationRuleGuessProbability,
            validationRuleSlipProbability,
            nameof(validationRuleGuessProbability));
        ValidateObservation(
            taskTagGuessProbability,
            taskTagSlipProbability,
            nameof(taskTagGuessProbability));

        if (validationRuleGuessProbability > taskTagGuessProbability
            || validationRuleSlipProbability > taskTagSlipProbability)
        {
            throw new ArgumentException("Точное правило не может быть менее надёжным, чем навык-тег.");
        }

        ArgumentNullException.ThrowIfNull(ahpMatrix);
        var matrix = ahpMatrix.ToArray();
        var weights = AhpCalculator.CalculateWeights(matrix);

        InitialMasteryProbability = initialMasteryProbability;
        LearnProbability = learnProbability;
        ValidationRuleGuessProbability = validationRuleGuessProbability;
        ValidationRuleSlipProbability = validationRuleSlipProbability;
        TaskTagGuessProbability = taskTagGuessProbability;
        TaskTagSlipProbability = taskTagSlipProbability;
        AhpMatrix = matrix;
        AhpWeights = weights;
    }

    /// <summary>Начальная вероятность освоения.</summary>
    public decimal InitialMasteryProbability { get; }

    /// <summary>Вероятность обучения.</summary>
    public decimal LearnProbability { get; }

    /// <summary>Вероятность угадывания для точного правила.</summary>
    public decimal ValidationRuleGuessProbability { get; }

    /// <summary>Вероятность ошибки для точного правила.</summary>
    public decimal ValidationRuleSlipProbability { get; }

    /// <summary>Вероятность угадывания для навыка-тега.</summary>
    public decimal TaskTagGuessProbability { get; }

    /// <summary>Вероятность ошибки для навыка-тега.</summary>
    public decimal TaskTagSlipProbability { get; }

    /// <summary>Матрица МАИ в построчном порядке.</summary>
    public IReadOnlyList<decimal> AhpMatrix { get; }

    /// <summary>Нормализованные веса критериев.</summary>
    public IReadOnlyList<decimal> AhpWeights { get; }

    private static void ValidateProbability(decimal value, string parameterName)
    {
        if (value is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Вероятность должна находиться в диапазоне от 0 до 1.");
        }
    }

    private static void ValidateObservation(decimal guess, decimal slip, string parameterName)
    {
        ValidateProbability(guess, parameterName);
        ValidateProbability(slip, parameterName);
        if (guess + slip >= 1)
        {
            throw new ArgumentException("Сумма вероятностей угадывания и ошибки должна быть меньше 1.", parameterName);
        }
    }

}
