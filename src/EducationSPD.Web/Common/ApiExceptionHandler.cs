using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EducationSPD.Application.Recommendations;

namespace EducationSPD.Web.Common;

/// <summary>
/// Преобразует ожидаемые и непредвиденные исключения в безопасный ProblemDetails.
/// </summary>
internal sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService problemDetailsService;
    private readonly ILogger<ApiExceptionHandler> logger;

    /// <summary>
    /// Создаёт обработчик ошибок API.
    /// </summary>
    public ApiExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<ApiExceptionHandler> logger)
    {
        this.problemDetailsService = problemDetailsService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is RecommendationValidationException validationException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ValidationProblemDetails(
                    validationException.Errors.ToDictionary(pair => pair.Key, pair => pair.Value))
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "Ошибка проверки запроса",
                    Detail = "Исправьте ссылки на навыки кандидатов."
                },
                Exception = exception
            });
        }

        var (statusCode, title, detail) = exception switch
        {
            BadHttpRequestException => (
                StatusCodes.Status400BadRequest,
                "Некорректный запрос",
                "Проверьте формат и значения параметров запроса."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Внутренняя ошибка сервера",
                "При обработке запроса произошла непредвиденная ошибка.")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Непредвиденная ошибка при обработке {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            },
            Exception = exception
        });
    }
}
