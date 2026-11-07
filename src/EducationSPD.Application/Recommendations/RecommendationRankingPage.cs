using EducationSPD.Domain.Recommendations;

namespace EducationSPD.Application.Recommendations;

/// <summary>Результат ранжирования для одной версии модели.</summary>
public sealed record RecommendationRankingPage(
    int ModelVersion,
    IReadOnlyList<RankedCandidate> Items);
