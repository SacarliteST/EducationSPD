namespace EducationSPD.Contracts.Recommendations;

/// <summary>Запрос персонального ранжирования заданий одного модуля.</summary>
public sealed record RankRecommendationsRequest(
    Guid? StudentId,
    string? ModuleKey,
    IReadOnlyList<RecommendationCandidateRequest>? Candidates);
