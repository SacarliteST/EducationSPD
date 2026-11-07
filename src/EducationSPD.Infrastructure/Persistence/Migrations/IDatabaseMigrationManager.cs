namespace EducationSPD.Infrastructure.Persistence.Migrations;

/// <summary>
/// Применяет ожидающие миграции базы данных.
/// </summary>
public interface IDatabaseMigrationManager
{
    /// <summary>
    /// Обновляет схему базы данных до текущей версии.
    /// </summary>
    Task MigrateAsync(CancellationToken cancellationToken = default);
}
