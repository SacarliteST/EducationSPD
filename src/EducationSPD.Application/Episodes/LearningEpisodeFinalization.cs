namespace EducationSPD.Application.Episodes;

/// <summary>Проверенные данные окончательной финализации учебного эпизода.</summary>
public sealed record LearningEpisodeFinalization(
    Guid EventId,
    Guid EpisodeId,
    Guid StudentId,
    string ModuleKey,
    Guid TaskId,
    DateTimeOffset FinalizedAt,
    IReadOnlyList<LearningEpisodeSkillResult> SkillResults);
