namespace EducationSPD.Contracts.Recommendations;

/// <summary>Задание-кандидат, уже отфильтрованное учебным модулем.</summary>
public sealed record RecommendationCandidateRequest(
    Guid? TaskId,
    short? DifficultyLevel,
    IReadOnlyList<RecommendationSkillRequest>? Skills);
