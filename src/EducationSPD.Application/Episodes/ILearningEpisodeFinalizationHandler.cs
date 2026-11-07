namespace EducationSPD.Application.Episodes;

/// <summary>Применяет окончательный результат учебного эпизода.</summary>
public interface ILearningEpisodeFinalizationHandler
{
    /// <summary>Идемпотентно сохраняет эпизод и все его наблюдения.</summary>
    Task<LearningEpisodeApplyResult> ApplyAsync(
        LearningEpisodeFinalization finalization,
        CancellationToken cancellationToken);
}
