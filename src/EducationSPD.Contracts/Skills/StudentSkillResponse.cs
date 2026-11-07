namespace EducationSPD.Contracts.Skills;

/// <summary>Текущее вероятностное состояние одного навыка.</summary>
public sealed record StudentSkillResponse(
    string SkillId,
    string Name,
    string? Description,
    StudentSkillOrigin Origin,
    bool IsDeleted,
    decimal MasteryProbability,
    int ObservationsCount,
    DateTimeOffset? LastObservedAt);
