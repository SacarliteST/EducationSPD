namespace EducationSPD.Domain.Recommendations;

/// <summary>Переданное модулем задание, подготовленное к ранжированию.</summary>
public sealed record RankingCandidate(
    Guid TaskId,
    short DifficultyLevel,
    int FinalizedEpisodeCount,
    IReadOnlyList<RankingCandidateSkill> Skills);
