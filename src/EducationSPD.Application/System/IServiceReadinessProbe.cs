namespace EducationSPD.Application.System;

/// <summary>Проверяет готовность инфраструктурных зависимостей сервиса.</summary>
public interface IServiceReadinessProbe
{
    /// <summary>Возвращает готовность PostgreSQL, миграций и Kafka consumers.</summary>
    Task<bool> IsReadyAsync(CancellationToken cancellationToken);
}
