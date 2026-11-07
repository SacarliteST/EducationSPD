namespace EducationSPD.Infrastructure.Configuration;

/// <summary>
/// Стартовые параметры математической модели.
/// </summary>
public sealed class ModelOptions
{
    /// <summary>Имя секции конфигурации.</summary>
    public const string SectionName = "Model";

    /// <summary>Начальная вероятность освоения навыка.</summary>
    public decimal InitialMasteryProbability { get; set; }

    /// <summary>Вероятность обучения после одной учебной возможности.</summary>
    public decimal LearnProbability { get; set; }

    /// <summary>Вероятность угадывания для точного правила.</summary>
    public decimal ValidationRuleGuessProbability { get; set; }

    /// <summary>Вероятность ошибки для точного правила.</summary>
    public decimal ValidationRuleSlipProbability { get; set; }

    /// <summary>Вероятность угадывания для навыка-тега.</summary>
    public decimal TaskTagGuessProbability { get; set; }

    /// <summary>Вероятность ошибки для навыка-тега.</summary>
    public decimal TaskTagSlipProbability { get; set; }

    /// <summary>Матрица попарных сравнений критериев МАИ.</summary>
    public decimal[][] AhpMatrix { get; set; } = [];

    internal static bool IsValid(ModelOptions options)
    {
        return IsProbability(options.InitialMasteryProbability)
            && IsProbability(options.LearnProbability)
            && IsReliableObservation(
                options.ValidationRuleGuessProbability,
                options.ValidationRuleSlipProbability)
            && IsReliableObservation(
                options.TaskTagGuessProbability,
                options.TaskTagSlipProbability)
            && options.ValidationRuleGuessProbability <= options.TaskTagGuessProbability
            && options.ValidationRuleSlipProbability <= options.TaskTagSlipProbability
            && IsFourByFourPositiveMatrix(options.AhpMatrix);
    }

    private static bool IsProbability(decimal value) => value is >= 0 and <= 1;

    private static bool IsReliableObservation(decimal guess, decimal slip) =>
        IsProbability(guess) && IsProbability(slip) && guess + slip < 1;

    private static bool IsFourByFourPositiveMatrix(decimal[][] matrix) =>
        matrix.Length == 4
        && matrix.All(row => row.Length == 4 && row.All(value => value > 0));
}
