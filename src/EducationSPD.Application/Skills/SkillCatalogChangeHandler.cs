using EducationSPD.Domain.Skills;

namespace EducationSPD.Application.Skills;

internal sealed class SkillCatalogChangeHandler(ISkillRepository repository)
    : ISkillCatalogChangeHandler
{
    public async Task<bool> ApplyAsync(
        SkillCatalogChange change,
        CancellationToken cancellationToken)
    {
        var skill = await repository.FindAsync(
            change.ModuleKey,
            change.SkillId,
            cancellationToken);

        if (skill is null)
        {
            skill = Skill.Create(
                Guid.NewGuid(),
                change.ModuleKey,
                change.SkillId,
                change.Name,
                change.Description,
                change.Origin,
                change.IsDeleted,
                change.OccurredAt,
                change.EventId);
            await repository.AddAsync(skill, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            return true;
        }

        var changed = skill.ApplyChange(
            change.Name,
            change.Description,
            change.Origin,
            change.IsDeleted,
            change.OccurredAt,
            change.EventId);
        await repository.SaveChangesAsync(cancellationToken);
        return changed;
    }
}
