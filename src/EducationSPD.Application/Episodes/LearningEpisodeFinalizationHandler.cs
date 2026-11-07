using EducationSPD.Application.Skills;
using EducationSPD.Domain.Episodes;

namespace EducationSPD.Application.Episodes;

internal sealed class LearningEpisodeFinalizationHandler(
    ILearningEpisodeRepository episodeRepository,
    ISkillRepository skillRepository) : ILearningEpisodeFinalizationHandler
{
    public async Task<LearningEpisodeApplyResult> ApplyAsync(
        LearningEpisodeFinalization finalization,
        CancellationToken cancellationToken)
    {
        if (await episodeRepository.FindByEventIdAsync(
                finalization.EventId,
                cancellationToken) is not null)
        {
            return LearningEpisodeApplyResult.Duplicate;
        }

        if (await episodeRepository.FindAsync(
                finalization.ModuleKey,
                finalization.EpisodeId,
                cancellationToken) is not null)
        {
            return LearningEpisodeApplyResult.Conflict;
        }

        var skillIds = finalization.SkillResults
            .Select(result => result.SkillId)
            .ToArray();
        var skills = await skillRepository.FindManyAsync(
            finalization.ModuleKey,
            skillIds,
            cancellationToken);
        if (skills.Count != skillIds.Length)
        {
            return LearningEpisodeApplyResult.MissingCatalog;
        }

        var skillsByExternalId = skills.ToDictionary(skill => skill.SkillId, StringComparer.Ordinal);
        var observations = finalization.SkillResults
            .Select(result => new NewSkillObservation(
                Guid.NewGuid(),
                skillsByExternalId[result.SkillId].Id,
                result.Source,
                result.Outcome))
            .ToArray();
        var episode = LearningEpisode.Create(
            Guid.NewGuid(),
            finalization.EventId,
            finalization.EpisodeId,
            finalization.StudentId,
            finalization.ModuleKey,
            finalization.TaskId,
            finalization.FinalizedAt,
            observations);

        await episodeRepository.AddAsync(episode, cancellationToken);
        await episodeRepository.SaveChangesAsync(cancellationToken);
        return LearningEpisodeApplyResult.Applied;
    }
}
