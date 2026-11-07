using EducationSPD.Domain.Skills;

namespace EducationSPD.Application.Skills;

/// <summary>Проверенное изменение локальной проекции каталога навыков.</summary>
public sealed record SkillCatalogChange(
    Guid EventId,
    string ModuleKey,
    string SkillId,
    string Name,
    string? Description,
    SkillOrigin Origin,
    bool IsDeleted,
    DateTimeOffset OccurredAt);
