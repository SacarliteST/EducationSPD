using EducationSPD.Domain.Episodes;

namespace EducationSPD.Application.Episodes;

/// <summary>Хранилище неизменяемой истории учебных эпизодов.</summary>
public interface ILearningEpisodeRepository
{
    /// <summary>Находит эпизод по идентификатору интеграционного события.</summary>
    Task<LearningEpisode?> FindByEventIdAsync(Guid eventId, CancellationToken cancellationToken);

    /// <summary>Находит эпизод по идентичности внутри учебного модуля.</summary>
    Task<LearningEpisode?> FindAsync(
        string moduleKey,
        Guid episodeId,
        CancellationToken cancellationToken);

    /// <summary>Добавляет окончательный эпизод со всеми наблюдениями.</summary>
    Task AddAsync(LearningEpisode episode, CancellationToken cancellationToken);

    /// <summary>Сохраняет добавленный эпизод.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
