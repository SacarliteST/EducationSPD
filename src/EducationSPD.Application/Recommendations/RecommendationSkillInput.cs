using EducationSPD.Domain.Episodes;

namespace EducationSPD.Application.Recommendations;

/// <summary>Проверенный навык кандидата.</summary>
public sealed record RecommendationSkillInput(
    string SkillId,
    SkillObservationSource Source);
