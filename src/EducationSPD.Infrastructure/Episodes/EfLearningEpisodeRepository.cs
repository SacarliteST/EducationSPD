using EducationSPD.Application.Episodes;
using EducationSPD.Domain.Episodes;
using EducationSPD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Infrastructure.Episodes;

internal sealed class EfLearningEpisodeRepository(AppDbContext dbContext)
    : ILearningEpisodeRepository
{
    public Task<LearningEpisode?> FindByEventIdAsync(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        return dbContext.LearningEpisodes
            .Include(episode => episode.Observations)
            .SingleOrDefaultAsync(episode => episode.EventId == eventId, cancellationToken);
    }

    public Task<LearningEpisode?> FindAsync(
        string moduleKey,
        Guid episodeId,
        CancellationToken cancellationToken)
    {
        return dbContext.LearningEpisodes
            .Include(episode => episode.Observations)
            .SingleOrDefaultAsync(
                episode => episode.ModuleKey == moduleKey && episode.EpisodeId == episodeId,
                cancellationToken);
    }

    public async Task AddAsync(
        LearningEpisode episode,
        CancellationToken cancellationToken)
    {
        await dbContext.LearningEpisodes.AddAsync(episode, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
