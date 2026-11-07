namespace EducationSPD.Contracts.Recommendations;

/// <summary>Ранжированное задание с объяснением.</summary>
public sealed record RankedCandidateResponse(
    Guid TaskId,
    int Rank,
    decimal RankScore,
    IReadOnlyList<RecommendationReasonResponse> Reasons);
