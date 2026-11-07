namespace EducationSPD.Application.Recommendations;

/// <summary>Каталог и персональная проекция навыка для ранжирования.</summary>
public sealed record RecommendationSkillContext(
    string SkillId,
    bool IsDeleted,
    decimal? MasteryProbability);
