using EducationSPD.Domain.Skills;

namespace EducationSPD.Application.Models;

/// <summary>Состояние навыка для выдачи доверенному учебному модулю.</summary>
public sealed record StudentSkillReadItem(
    string SkillId,
    string Name,
    string? Description,
    SkillOrigin Origin,
    bool IsDeleted,
    decimal MasteryProbability,
    int ObservationsCount,
    DateTimeOffset? LastObservedAt);
