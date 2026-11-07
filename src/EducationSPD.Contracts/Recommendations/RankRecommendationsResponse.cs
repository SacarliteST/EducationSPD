namespace EducationSPD.Contracts.Recommendations;

/// <summary>Результат ранжирования для активной версии модели.</summary>
public sealed record RankRecommendationsResponse(
    int ModelVersion,
    IReadOnlyList<RankedCandidateResponse> Items);
