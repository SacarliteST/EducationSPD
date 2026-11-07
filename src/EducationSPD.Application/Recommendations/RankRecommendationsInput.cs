namespace EducationSPD.Application.Recommendations;

/// <summary>Проверенный запрос ранжирования одного модуля.</summary>
public sealed record RankRecommendationsInput(
    Guid StudentId,
    string ModuleKey,
    IReadOnlyList<RecommendationCandidateInput> Candidates);
