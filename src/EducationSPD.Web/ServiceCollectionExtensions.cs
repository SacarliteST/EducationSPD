using System.Text.Json.Serialization;
using EducationSPD.Web.Common;
using EducationSPD.Web.Security;
using EducationSPD.Web.Features.Skills;
using EducationSPD.Contracts.Skills;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EducationSPD.Web;

/// <summary>
/// Регистрирует HTTP transport EducationSPD.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет контроллеры, валидацию, обработку ошибок и сервисную авторизацию.
    /// </summary>
    public static IServiceCollection AddWeb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<ApiExceptionHandler>();
        services.AddScoped<FluentValidationActionFilter>();
        services.AddScoped<ServiceKeyAuthorizationFilter>();
        services.AddScoped<IValidator<GetStudentSkillsRequest>, GetStudentSkillsRequestValidator>();

        services.AddOptions<ServiceAuthenticationOptions>()
            .Bind(configuration.GetSection(ServiceAuthenticationOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ServiceKey),
                $"{ServiceAuthenticationOptions.SectionName}:ServiceKey обязателен.")
            .ValidateOnStart();

        services
            .AddControllers(options =>
            {
                options.Filters.Add<FluentValidationActionFilter>();
                options.Filters.Add<ServiceKeyAuthorizationFilter>();
            })
            .AddApplicationPart(typeof(IWebMarkerInterface).Assembly)
            .AddControllersAsServices()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = ApiValidationProblemFactory.Create;
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        return services;
    }
}
