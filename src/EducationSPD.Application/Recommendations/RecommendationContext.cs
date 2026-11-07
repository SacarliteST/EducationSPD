namespace EducationSPD.Application.Recommendations;

/// <summary>Контекст всех кандидатов, загруженный пакетно.</summary>
public sealed record RecommendationContext(
    IReadOnlyDictionary<string, RecommendationSkillContext> Skills,
    IReadOnlyDictionary<Guid, int> FinalizedEpisodeCounts);
