using EducationSPD.Application.Models;
using EducationSPD.Domain.Recommendations;

namespace EducationSPD.Application.Recommendations;

internal sealed class RecommendationRankingHandler : IRecommendationRankingHandler
{
    private readonly IModelVersionRepository modelVersionRepository;
    private readonly IRecommendationContextReader contextReader;

    public RecommendationRankingHandler(
        IModelVersionRepository modelVersionRepository,
        IRecommendationContextReader contextReader)
    {
        this.modelVersionRepository = modelVersionRepository;
        this.contextReader = contextReader;
    }

    public async Task<RecommendationRankingPage> RankAsync(
        RankRecommendationsInput input,
        CancellationToken cancellationToken)
    {
        var activeModel = await modelVersionRepository.GetActiveAsync(cancellationToken)
            ?? throw new InvalidOperationException("Активная версия математической модели не найдена.");
        var skillIds = input.Candidates
            .SelectMany(candidate => candidate.Skills)
            .Select(skill => skill.SkillId)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var taskIds = input.Candidates.Select(candidate => candidate.TaskId).ToArray();
        var context = await contextReader.ReadAsync(
            input.StudentId,
            input.ModuleKey,
            activeModel.Version,
            skillIds,
            taskIds,
            cancellationToken);

        EnsureSkillsAvailable(skillIds, context.Skills);
        var candidates = input.Candidates.Select(candidate => new RankingCandidate(
            candidate.TaskId,
            candidate.DifficultyLevel,
            context.FinalizedEpisodeCounts.GetValueOrDefault(candidate.TaskId),
            candidate.Skills.Select(skill => new RankingCandidateSkill(
                skill.SkillId,
                skill.Source,
                context.Skills[skill.SkillId].MasteryProbability)).ToArray())).ToArray();
        var ranked = RecommendationRanker.Rank(candidates, activeModel.GetParameters());
        return new RecommendationRankingPage(activeModel.Version, ranked);
    }

    private static void EnsureSkillsAvailable(
        IReadOnlyCollection<string> requestedSkillIds,
        IReadOnlyDictionary<string, RecommendationSkillContext> skills)
    {
        var missing = requestedSkillIds.Where(skillId => !skills.ContainsKey(skillId)).ToArray();
        var deleted = requestedSkillIds
            .Where(skillId => skills.TryGetValue(skillId, out var skill) && skill.IsDeleted)
            .ToArray();
        var errors = new Dictionary<string, string[]>();
        if (missing.Length > 0)
        {
            errors["Candidates.Skills"] =
                [$"Навыки отсутствуют в каталоге модуля: {string.Join(", ", missing)}."];
        }

        if (deleted.Length > 0)
        {
            errors["Candidates.Skills.Deleted"] =
                [$"Удалённые навыки нельзя использовать для нового ранжирования: {string.Join(", ", deleted)}."];
        }

        if (errors.Count > 0)
        {
            throw new RecommendationValidationException(errors);
        }
    }
}
