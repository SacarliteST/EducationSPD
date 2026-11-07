using EducationSPD.Application.Episodes;
using EducationSPD.Application.Inbox;
using EducationSPD.Application.Skills;
using EducationSPD.Infrastructure.Configuration;
using EducationSPD.Infrastructure.Episodes;
using EducationSPD.Infrastructure.Inbox;
using EducationSPD.Infrastructure.Kafka;
using EducationSPD.Infrastructure.Persistence;
using EducationSPD.Infrastructure.Persistence.Migrations;
using EducationSPD.Infrastructure.Skills;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EducationSPD.Infrastructure;

/// <summary>
/// Регистрирует хранение и интеграционные зависимости слоя Infrastructure.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет инфраструктуру EducationSPD.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<PostgresOptions>()
            .Configure(options =>
            {
                options.ConnectionString = configuration.GetConnectionString("Postgres") ?? string.Empty;
                options.ApplyMigrationsOnStartup = configuration.GetValue(
                    "Database:ApplyMigrationsOnStartup",
                    false);
            })
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                "ConnectionStrings:Postgres обязателен.")
            .ValidateOnStart();

        services.AddOptions<KafkaOptions>()
            .BindConfiguration(KafkaOptions.SectionName)
            .Validate(KafkaOptions.IsValid, "Конфигурация Kafka заполнена некорректно.")
            .ValidateOnStart();

        services.AddOptions<InboxOptions>()
            .BindConfiguration(InboxOptions.SectionName)
            .Validate(InboxOptions.IsValid, "Конфигурация Inbox заполнена некорректно.")
            .ValidateOnStart();

        services.AddOptions<ModelOptions>()
            .BindConfiguration(ModelOptions.SectionName)
            .Validate(ModelOptions.IsValid, "Конфигурация математической модели заполнена некорректно.")
            .ValidateOnStart();

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var postgresOptions = serviceProvider.GetRequiredService<IOptions<PostgresOptions>>().Value;
            options.UseNpgsql(postgresOptions.ConnectionString);
        });

        services.AddScoped<IDatabaseMigrationManager, DatabaseMigrationManager>();
        services.AddScoped<ILearningEpisodeRepository, EfLearningEpisodeRepository>();
        services.AddScoped<IStudentModuleLock, PostgresStudentModuleLock>();
        services.AddScoped<IInboxStore, EfInboxStore>();
        services.AddScoped<IInboxBatchExecutor, EfInboxBatchExecutor>();
        services.AddScoped<IPendingCatalogNotifier, EfPendingCatalogNotifier>();
        services.AddScoped<ISkillRepository, EfSkillRepository>();
        services.AddHostedService<SkillCatalogKafkaConsumer>();
        services.AddHostedService<SkillCatalogInboxProcessor>();
        services.AddHostedService<LearningEpisodeKafkaConsumer>();
        services.AddHostedService<LearningEpisodeInboxProcessor>();

        return services;
    }
}
