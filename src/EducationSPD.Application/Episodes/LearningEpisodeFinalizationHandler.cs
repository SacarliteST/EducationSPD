using EducationSPD.Application.Models;
using EducationSPD.Application.Skills;
using EducationSPD.Domain.Episodes;
using EducationSPD.Domain.Models;

namespace EducationSPD.Application.Episodes;

internal sealed class LearningEpisodeFinalizationHandler(
    ILearningEpisodeRepository episodeRepository,
    ISkillRepository skillRepository,
    IStudentModuleLock studentModuleLock,
    IModelVersionRepository modelVersionRepository,
    IStudentSkillStateRepository stateRepository,
    TimeProvider timeProvider) : ILearningEpisodeFinalizationHandler
{
    public async Task<LearningEpisodeApplyResult> ApplyAsync(
        LearningEpisodeFinalization finalization,
        CancellationToken cancellationToken)
    {
        await studentModuleLock.AcquireAsync(
            finalization.StudentId,
            finalization.ModuleKey,
            cancellationToken);

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

        var activeModel = await modelVersionRepository.GetActiveAsync(cancellationToken)
            ?? throw new InvalidOperationException("Активная версия математической модели не найдена.");

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
            activeModel.Version,
            observations);

        var history = await episodeRepository.ListChronologicalAsync(
            finalization.StudentId,
            finalization.ModuleKey,
            cancellationToken);
        var completeHistory = history
            .Append(episode)
            .OrderBy(item => item.FinalizedAt)
            .ThenBy(item => item.EpisodeId)
            .ToArray();

        await episodeRepository.AddAsync(episode, cancellationToken);
        foreach (var skillId in observations.Select(observation => observation.SkillId))
        {
            await RebuildSkillProjectionAsync(
                finalization.StudentId,
                finalization.ModuleKey,
                skillId,
                activeModel,
                completeHistory,
                cancellationToken);
        }

        await episodeRepository.SaveChangesAsync(cancellationToken);
        return LearningEpisodeApplyResult.Applied;
    }

    private async Task RebuildSkillProjectionAsync(
        Guid studentId,
        string moduleKey,
        Guid skillId,
        ModelVersion activeModel,
        IReadOnlyList<LearningEpisode> history,
        CancellationToken cancellationToken)
    {
        var parameters = activeModel.GetParameters();
        var mastery = parameters.InitialMasteryProbability;
        var observationsCount = 0;
        DateTimeOffset? lastObservedAt = null;

        foreach (var episode in history)
        {
            var observation = episode.Observations.SingleOrDefault(item => item.SkillId == skillId);
            if (observation is null)
            {
                continue;
            }

            mastery = BktCalculator.Calculate(mastery, observation.Outcome, observation.Source, parameters);
            observationsCount++;
            lastObservedAt = episode.FinalizedAt;
        }

        var state = await stateRepository.FindAsync(
            studentId,
            moduleKey,
            skillId,
            activeModel.Version,
            cancellationToken);
        if (state is null)
        {
            state = StudentSkillState.Create(studentId, moduleKey, skillId, activeModel.Version);
            await stateRepository.AddAsync(state, cancellationToken);
        }

        state.Rebuild(
            mastery,
            observationsCount,
            lastObservedAt!.Value,
            timeProvider.GetUtcNow());
    }
}
