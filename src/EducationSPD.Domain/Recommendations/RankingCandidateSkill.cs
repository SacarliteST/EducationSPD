using EducationSPD.Domain.Episodes;

namespace EducationSPD.Domain.Recommendations;

/// <summary>Навык кандидата вместе с доступным состоянием студента.</summary>
public sealed record RankingCandidateSkill(
    string SkillId,
    SkillObservationSource Source,
    decimal? MasteryProbability);
