using Microsoft.Extensions.DependencyInjection;
using EducationSPD.Application.Skills;

namespace EducationSPD.Application;

/// <summary>
/// Регистрирует сценарии и общие зависимости слоя Application.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет сервисы слоя Application.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ISkillCatalogChangeHandler, SkillCatalogChangeHandler>();

        return services;
    }
}
