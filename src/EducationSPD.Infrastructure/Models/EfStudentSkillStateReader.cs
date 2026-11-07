using EducationSPD.Application.Models;
using EducationSPD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Infrastructure.Models;

internal sealed class EfStudentSkillStateReader : IStudentSkillStateReader
{
    private readonly AppDbContext dbContext;

    public EfStudentSkillStateReader(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<StudentSkillReadSlice> ReadAsync(
        string moduleKey,
        Guid studentId,
        int modelVersion,
        decimal initialMasteryProbability,
        bool includeDeleted,
        int offset,
        int limit,
        CancellationToken cancellationToken)
    {
        var skills = dbContext.Skills
            .AsNoTracking()
            .Where(skill => skill.ModuleKey == moduleKey);
        if (!includeDeleted)
        {
            skills = skills.Where(skill => !skill.IsDeleted);
        }

        var states = dbContext.StudentSkillStates
            .AsNoTracking()
            .Where(state => state.StudentId == studentId
                && state.ModuleKey == moduleKey
                && state.ModelVersion == modelVersion);
        var query =
            from skill in skills
            join state in states on skill.Id equals state.SkillId into skillStates
            from state in skillStates.DefaultIfEmpty()
            orderby skill.SkillId, skill.Id
            select new StudentSkillReadItem(
                skill.SkillId,
                skill.Name,
                skill.Description,
                skill.Origin,
                skill.IsDeleted,
                state == null ? initialMasteryProbability : state.MasteryProbability,
                state == null ? 0 : state.ObservationsCount,
                state == null ? null : state.LastObservedAt);

        var count = await skills.CountAsync(cancellationToken);
        var items = await query
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);
        return new StudentSkillReadSlice(count, items);
    }
}
