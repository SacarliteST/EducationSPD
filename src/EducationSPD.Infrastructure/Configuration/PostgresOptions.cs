namespace EducationSPD.Infrastructure.Configuration;

/// <summary>
/// Настройки подключения к PostgreSQL и применения миграций.
/// </summary>
public sealed class PostgresOptions
{
    /// <summary>
    /// Строка подключения из <c>ConnectionStrings:Postgres</c>.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Применять ли миграции при запуске вне Development.
    /// </summary>
    public bool ApplyMigrationsOnStartup { get; set; }
}
