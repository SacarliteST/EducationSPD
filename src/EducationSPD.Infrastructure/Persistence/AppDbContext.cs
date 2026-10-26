using EducationSPD.Domain.Examples;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Infrastructure.Persistence;

/// <summary>
/// Контекст базы данных приложения.
/// </summary>
public sealed class AppDbContext : DbContext
{
    /// <summary>Создаёт экземпляр класса <see cref="AppDbContext"/>.</summary>
    /// <param name="options">Параметры подключения и настройки Entity Framework Core.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Примеры.
    /// </summary>
    public DbSet<Example> Examples => Set<Example>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
