using EducationSPD.Application.Recommendations;
using EducationSPD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Infrastructure.Recommendations;

internal sealed class EfRecommendationContextReader : IRecommendationContextReader
{
    private readonly AppDbContext dbContext;

    public EfRecommendationContextReader(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<RecommendationContext> ReadAsync(
        Guid studentId,
        string moduleKey,
        int modelVersion,
        IReadOnlyCollection<string> skillIds,
        IReadOnlyCollection<Guid> taskIds,
        CancellationToken cancellationToken)
    {
        var states = dbContext.StudentSkillStates
            .AsNoTracking()
            .Where(state => state.StudentId == studentId
                && state.ModuleKey == moduleKey
                && state.ModelVersion == modelVersion);
        var skillQuery =
            from skill in dbContext.Skills.AsNoTracking()
            where skill.ModuleKey == moduleKey && skillIds.Contains(skill.SkillId)
            join state in states on skill.Id equals state.SkillId into skillStates
            from state in skillStates.DefaultIfEmpty()
            select new RecommendationSkillContext(
                skill.SkillId,
                skill.IsDeleted,
                state == null ? null : state.MasteryProbability);
        var skills = await skillQuery.ToDictionaryAsync(
            skill => skill.SkillId,
            StringComparer.Ordinal,
            cancellationToken);
        var episodeCounts = await dbContext.LearningEpisodes
            .AsNoTracking()
            .Where(episode => episode.StudentId == studentId
                && episode.ModuleKey == moduleKey
                && taskIds.Contains(episode.TaskId))
            .GroupBy(episode => episode.TaskId)
            .Select(group => new { TaskId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.TaskId, item => item.Count, cancellationToken);

        return new RecommendationContext(skills, episodeCounts);
    }
}
