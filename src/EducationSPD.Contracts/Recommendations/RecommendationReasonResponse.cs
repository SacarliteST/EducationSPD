namespace EducationSPD.Contracts.Recommendations;

/// <summary>Вклад одного критерия в итоговый балл.</summary>
public sealed record RecommendationReasonResponse(
    RecommendationReasonCodeResponse Code,
    decimal Contribution,
    IReadOnlyList<string> SkillIds);
