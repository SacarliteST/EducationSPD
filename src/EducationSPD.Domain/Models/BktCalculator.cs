using EducationSPD.Domain.Episodes;

namespace EducationSPD.Domain.Models;

/// <summary>Детерминированно обновляет вероятность освоения навыка по модели BKT.</summary>
public static class BktCalculator
{
    /// <summary>Применяет одно бинарное наблюдение и переход обучения.</summary>
    public static decimal Calculate(
        decimal previousMastery,
        SkillObservationOutcome outcome,
        SkillObservationSource source,
        ModelParameters parameters)
    {
        if (previousMastery is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(previousMastery),
                "Вероятность освоения должна находиться в диапазоне от 0 до 1.");
        }

        ArgumentNullException.ThrowIfNull(parameters);
        var (guess, slip) = SelectObservationParameters(source, parameters);
        var posterior = outcome switch
        {
            SkillObservationOutcome.Succeeded => CalculateSuccessPosterior(previousMastery, guess, slip),
            SkillObservationOutcome.Failed => CalculateFailurePosterior(previousMastery, guess, slip),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), "Результат наблюдения не поддерживается."),
        };

        var nextMastery = posterior + ((1 - posterior) * parameters.LearnProbability);
        return Math.Clamp(nextMastery, 0, 1);
    }

    private static (decimal Guess, decimal Slip) SelectObservationParameters(
        SkillObservationSource source,
        ModelParameters parameters)
    {
        return source switch
        {
            SkillObservationSource.ValidationRule =>
                (parameters.ValidationRuleGuessProbability, parameters.ValidationRuleSlipProbability),
            SkillObservationSource.TaskTag =>
                (parameters.TaskTagGuessProbability, parameters.TaskTagSlipProbability),
            _ => throw new ArgumentOutOfRangeException(nameof(source), "Источник наблюдения не поддерживается."),
        };
    }

    private static decimal CalculateSuccessPosterior(decimal previousMastery, decimal guess, decimal slip)
    {
        var masteredEvidence = previousMastery * (1 - slip);
        var denominator = masteredEvidence + ((1 - previousMastery) * guess);

        // При нулевой вероятности обоих путей наблюдение противоречит модели.
        // Берём непрерывный предел формулы для успешного результата.
        return denominator == 0 ? 1 : masteredEvidence / denominator;
    }

    private static decimal CalculateFailurePosterior(decimal previousMastery, decimal guess, decimal slip)
    {
        var masteredEvidence = previousMastery * slip;
        var denominator = masteredEvidence + ((1 - previousMastery) * (1 - guess));

        // При нулевой вероятности обоих путей используем предел формулы для провала.
        return denominator == 0 ? 0 : masteredEvidence / denominator;
    }
}
