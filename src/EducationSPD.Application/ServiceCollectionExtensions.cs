using Microsoft.Extensions.DependencyInjection;
using EducationSPD.Application.Episodes;
using EducationSPD.Application.Skills;
using EducationSPD.Application.Models;
using EducationSPD.Application.Recommendations;

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
        services.AddScoped<ILearningEpisodeFinalizationHandler, LearningEpisodeFinalizationHandler>();
        services.AddScoped<IStudentSkillsQueryHandler, StudentSkillsQueryHandler>();
        services.AddScoped<IRecommendationRankingHandler, RecommendationRankingHandler>();

        return services;
    }
}
