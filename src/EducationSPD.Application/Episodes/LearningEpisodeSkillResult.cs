using EducationSPD.Domain.Episodes;

namespace EducationSPD.Application.Episodes;

/// <summary>Проверенный итог навыка финализированного эпизода.</summary>
public sealed record LearningEpisodeSkillResult(
    string SkillId,
    SkillObservationSource Source,
    SkillObservationOutcome Outcome);
