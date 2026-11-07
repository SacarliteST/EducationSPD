using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace EducationSPD.Web.Security;

/// <summary>
/// Ограничивает controller endpoints доверенным сервисным ключом.
/// </summary>
internal sealed class ServiceKeyAuthorizationFilter : IAsyncAuthorizationFilter
{
    internal const string HeaderName = "X-Service-Key";
    private readonly ServiceAuthenticationOptions options;

    /// <summary>
    /// Создаёт фильтр сервисной авторизации.
    /// </summary>
    public ServiceKeyAuthorizationFilter(IOptions<ServiceAuthenticationOptions> options)
    {
        this.options = options.Value;
    }

    /// <inheritdoc />
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            return Task.CompletedTask;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var values)
            || string.IsNullOrWhiteSpace(values.FirstOrDefault()))
        {
            context.Result = CreateProblem(
                StatusCodes.Status401Unauthorized,
                "Требуется аутентификация сервиса",
                "Передайте сервисный ключ в заголовке X-Service-Key.",
                "Authentication.Required");
            return Task.CompletedTask;
        }

        if (!KeysMatch(values.First()!, options.ServiceKey))
        {
            context.Result = CreateProblem(
                StatusCodes.Status403Forbidden,
                "Доступ запрещён",
                "Передан неверный сервисный ключ.",
                "Authentication.InvalidServiceKey");
        }

        return Task.CompletedTask;
    }

    private static bool KeysMatch(string actual, string expected)
    {
        var actualHash = SHA256.HashData(Encoding.UTF8.GetBytes(actual));
        var expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expected));

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static ObjectResult CreateProblem(int statusCode, string title, string detail, string code)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };
        problem.Extensions["code"] = code;

        return new ObjectResult(problem)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }
}
