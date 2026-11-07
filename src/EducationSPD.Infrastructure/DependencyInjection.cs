using EducationSPD.Application.Examples;
using EducationSPD.Infrastructure.Configuration;
using EducationSPD.Infrastructure.Examples;
using EducationSPD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EducationSPD.Infrastructure;

/// <summary>
/// Регистрация инфраструктурных зависимостей.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Добавляет PostgreSQL-контекст и репозитории в контейнер зависимостей.
    /// </summary>
    /// <param name="services">Коллекция сервисов приложения.</param>
    /// <param name="configuration">Конфигурация приложения с подключением к базе данных.</param>
    /// <returns>Та же коллекция сервисов для цепочки вызовов.</returns>
    public static IServiceCollection AddAppInfrastructure(
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

        services.AddScoped<IExamplesRepository, EfExamplesRepository>();

        return services;
    }
}
