namespace EducationSPD.Application.Recommendations;

/// <summary>Проверенное задание-кандидат.</summary>
public sealed record RecommendationCandidateInput(
    Guid TaskId,
    short DifficultyLevel,
    IReadOnlyList<RecommendationSkillInput> Skills);
