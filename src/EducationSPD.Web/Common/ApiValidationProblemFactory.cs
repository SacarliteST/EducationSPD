using Microsoft.AspNetCore.Mvc;

namespace EducationSPD.Web.Common;

/// <summary>
/// Создаёт единый ответ для ошибок model binding и DataAnnotations.
/// </summary>
internal static class ApiValidationProblemFactory
{
    /// <summary>
    /// Создаёт ответ 422 из текущего ModelState.
    /// </summary>
    public static IActionResult Create(ActionContext context)
    {
        var problem = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "Ошибка проверки запроса",
            Detail = "Исправьте значения указанных полей."
        };

        return new UnprocessableEntityObjectResult(problem);
    }
}
