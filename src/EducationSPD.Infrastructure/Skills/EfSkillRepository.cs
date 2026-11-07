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

    public async Task AddAsync(Skill skill, CancellationToken cancellationToken)
    {
        await dbContext.Skills.AddAsync(skill, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
