using EducationSPD.Domain.Models;

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
        try
        {
            _ = new ModelParameters(
                options.InitialMasteryProbability,
                options.LearnProbability,
                options.ValidationRuleGuessProbability,
                options.ValidationRuleSlipProbability,
                options.TaskTagGuessProbability,
                options.TaskTagSlipProbability,
                options.AhpMatrix.SelectMany(row => row).ToArray());
            return options.AhpMatrix.Length == ModelParameters.AhpMatrixSize
                && options.AhpMatrix.All(row => row.Length == ModelParameters.AhpMatrixSize);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
