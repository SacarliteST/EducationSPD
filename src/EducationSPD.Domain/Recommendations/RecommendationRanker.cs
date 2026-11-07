using EducationSPD.Domain.Episodes;
using EducationSPD.Domain.Models;

namespace EducationSPD.Domain.Recommendations;

/// <summary>Детерминированно ранжирует только переданные учебным модулем задания.</summary>
public static class RecommendationRanker
{
    /// <summary>Рассчитывает критерии, итоговые баллы и стабильные позиции.</summary>
    public static IReadOnlyList<RankedCandidate> Rank(
        IReadOnlyList<RankingCandidate> candidates,
        ModelParameters model)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(model);

        var scored = candidates.Select(candidate => Score(candidate, model)).ToArray();
        return scored
            .OrderByDescending(candidate => candidate.RankScore)
            .ThenBy(candidate => candidate.TaskId)
            .Select((candidate, index) => candidate with { Rank = index + 1 })
            .ToArray();
    }

    private static RankedCandidate Score(RankingCandidate candidate, ModelParameters model)
    {
        ValidateCandidate(candidate);
        var masteryValues = candidate.Skills
            .Select(skill => skill.MasteryProbability ?? model.InitialMasteryProbability)
            .ToArray();
        var averageMastery = masteryValues.Average();
        var knowledgeDeficit = 1 - averageMastery;
        var targetDifficulty = GetTargetDifficulty(averageMastery);
        var difficultyFit = 1 - (Math.Abs(candidate.DifficultyLevel - targetDifficulty) / 4m);
        var novelty = 1m / (1 + candidate.FinalizedEpisodeCount);
        var preciseSkills = candidate.Skills
            .Where(skill => skill.Source == SkillObservationSource.ValidationRule)
            .Select(skill => skill.SkillId)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var diagnosticValue = (decimal)preciseSkills.Length / candidate.Skills.Count;
        var allSkillIds = candidate.Skills
            .Select(skill => skill.SkillId)
            .Order(StringComparer.Ordinal)
            .ToArray();

        var reasons = new[]
        {
            CreateReason(RecommendationReasonCode.TargetsWeakSkills, model.AhpWeights[0], knowledgeDeficit, allSkillIds),
            CreateReason(RecommendationReasonCode.DifficultyMatchesCurrentLevel, model.AhpWeights[1], difficultyFit, allSkillIds),
            CreateReason(RecommendationReasonCode.TaskIsNewForStudent, model.AhpWeights[2], novelty, Array.Empty<string>()),
            CreateReason(RecommendationReasonCode.ProvidesPreciseAssessment, model.AhpWeights[3], diagnosticValue, preciseSkills),
        };
        return new RankedCandidate(candidate.TaskId, 0, reasons.Sum(reason => reason.Contribution), reasons);
    }

    private static RecommendationReason CreateReason(
        RecommendationReasonCode code,
        decimal weight,
        decimal criterion,
        IReadOnlyList<string> skillIds) =>
        new(code, weight * criterion, skillIds);

    private static short GetTargetDifficulty(decimal averageMastery)
    {
        return averageMastery switch
        {
            < 0.30m => 1,
            < 0.50m => 2,
            < 0.70m => 3,
            < 0.85m => 4,
            _ => 5,
        };
    }

    private static void ValidateCandidate(RankingCandidate candidate)
    {
        if (candidate.TaskId == Guid.Empty)
        {
            throw new ArgumentException("Идентификатор задания не может быть пустым.", nameof(candidate));
        }

        if (candidate.DifficultyLevel is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(candidate), "Сложность должна находиться в диапазоне от 1 до 5.");
        }

        if (candidate.FinalizedEpisodeCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(candidate), "Количество эпизодов не может быть отрицательным.");
        }

        if (candidate.Skills.Count == 0)
        {
            throw new ArgumentException("Кандидат должен содержать хотя бы один навык.", nameof(candidate));
        }

        foreach (var skill in candidate.Skills)
        {
            if (string.IsNullOrWhiteSpace(skill.SkillId))
            {
                throw new ArgumentException("Идентификатор навыка обязателен.", nameof(candidate));
            }

            if (!Enum.IsDefined(skill.Source))
            {
                throw new ArgumentOutOfRangeException(nameof(candidate), "Источник навыка не поддерживается.");
            }

            if (skill.MasteryProbability is < 0 or > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(candidate), "Вероятность освоения должна находиться в диапазоне от 0 до 1.");
            }
        }
    }
}
