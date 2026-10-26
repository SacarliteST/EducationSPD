using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EducationSPD.Web.Endpoints;

/// <summary>
/// Глобальный обработчик необработанных исключений: пишет ошибку в журнал
/// и возвращает клиенту <c>application/problem+json</c> без внутренних деталей.
/// </summary>
internal static class ExceptionHandlerExtensions
{
    /// <summary>Подключает глобальный обработчик необработанных исключений API.</summary>
    /// <param name="app">Приложение.</param>
    /// <returns>То же приложение.</returns>
    public static WebApplication UseApiExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(exceptionApp =>
        {
            exceptionApp.Run(async context =>
            {
                var feature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = feature?.Error;

                var statusCode = exception switch
                {
                    BadHttpRequestException badRequest => badRequest.StatusCode,
                    ArgumentException => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status500InternalServerError
                };

                var logger = context.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("EducationSPD.Web.ExceptionHandler");
                logger.LogError(exception, "Unhandled exception on {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";

                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = statusCode,
                    Title = GetTitle(statusCode),
                    Detail = GetDetail(statusCode)
                });
            });
        });

        return app;
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Некорректный запрос",
        _ => "Внутренняя ошибка сервера"
    };

    private static string GetDetail(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Проверьте формат и значения параметров запроса.",
        _ => "При обработке запроса произошла непредвиденная ошибка."
    };
}
