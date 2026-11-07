namespace EducationSPD.Domain.Recommendations;

/// <summary>Вклад одного критерия в итоговый балл рекомендации.</summary>
public sealed record RecommendationReason(
    RecommendationReasonCode Code,
    decimal Contribution,
    IReadOnlyList<string> SkillIds);
