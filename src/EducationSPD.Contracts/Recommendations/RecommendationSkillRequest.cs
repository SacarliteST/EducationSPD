namespace EducationSPD.Contracts.Recommendations;

/// <summary>Навык одного кандидата на ранжирование.</summary>
public sealed record RecommendationSkillRequest(
    string? SkillId,
    RecommendationSkillSource? Source);
