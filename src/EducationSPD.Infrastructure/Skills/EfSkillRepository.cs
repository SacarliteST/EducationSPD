using EducationSPD.Application.Skills;
using EducationSPD.Domain.Skills;
using EducationSPD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Infrastructure.Skills;

internal sealed class EfSkillRepository : ISkillRepository
{
    private readonly AppDbContext dbContext;

    public EfSkillRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<Skill?> FindAsync(
        string moduleKey,
        string skillId,
        CancellationToken cancellationToken)
    {
        return dbContext.Skills.SingleOrDefaultAsync(
            skill => skill.ModuleKey == moduleKey && skill.SkillId == skillId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Skill>> FindManyAsync(
        string moduleKey,
        IReadOnlyCollection<string> skillIds,
        CancellationToken cancellationToken)
    {
        return await dbContext.Skills
            .Where(skill => skill.ModuleKey == moduleKey && skillIds.Contains(skill.SkillId))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Skill skill, CancellationToken cancellationToken)
    {
        await dbContext.Skills.AddAsync(skill, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
