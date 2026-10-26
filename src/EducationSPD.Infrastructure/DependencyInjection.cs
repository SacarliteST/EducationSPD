using EducationSPD.Application.Examples;
using EducationSPD.Infrastructure.Examples;
using EducationSPD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IExamplesRepository, EfExamplesRepository>();

        return services;
    }
}
