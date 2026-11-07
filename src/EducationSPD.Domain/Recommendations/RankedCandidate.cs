namespace EducationSPD.Domain.Recommendations;

/// <summary>Позиция одного входного задания и объяснение её балла.</summary>
public sealed record RankedCandidate(
    Guid TaskId,
    int Rank,
    decimal RankScore,
    IReadOnlyList<RecommendationReason> Reasons);
