namespace EducationSPD.Application.Recommendations;

/// <summary>Пакетно загружает данные, необходимые для ранжирования.</summary>
public interface IRecommendationContextReader
{
    /// <summary>Читает каталог, проекции и количество эпизодов без N+1-запросов.</summary>
    Task<RecommendationContext> ReadAsync(
        Guid studentId,
        string moduleKey,
        int modelVersion,
        IReadOnlyCollection<string> skillIds,
        IReadOnlyCollection<Guid> taskIds,
        CancellationToken cancellationToken);
}
