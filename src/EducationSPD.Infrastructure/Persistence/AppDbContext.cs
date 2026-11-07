using EducationSPD.Domain.Inbox;
using EducationSPD.Domain.Episodes;
using EducationSPD.Domain.Skills;
using EducationSPD.Domain.Models;
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

    /// <summary>Локальный каталог навыков учебных модулей.</summary>
    public DbSet<Skill> Skills => Set<Skill>();

    /// <summary> Сохранённые интеграционные сообщения.</summary>
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    /// <summary>Неизменяемая история финализированных учебных эпизодов.</summary>
    public DbSet<LearningEpisode> LearningEpisodes => Set<LearningEpisode>();

    /// <summary>Бинарные наблюдения навыков завершённых эпизодов.</summary>
    public DbSet<SkillObservation> SkillObservations => Set<SkillObservation>();

    /// <summary>Версии математической модели.</summary>
    public DbSet<ModelVersion> ModelVersions => Set<ModelVersion>();

    /// <summary>Текущие BKT-проекции навыков студентов.</summary>
    public DbSet<StudentSkillState> StudentSkillStates => Set<StudentSkillState>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
