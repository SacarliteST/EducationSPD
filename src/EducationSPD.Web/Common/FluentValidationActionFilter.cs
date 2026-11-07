using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EducationSPD.Web.Common;

/// <summary>
/// Выполняет FluentValidation для аргументов controller action.
/// </summary>
internal sealed class FluentValidationActionFilter : IAsyncActionFilter
{
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Создаёт фильтр валидации.
    /// </summary>
    public FluentValidationActionFilter(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>();

        foreach (var argument in context.ActionArguments.Values.Where(value => value is not null))
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(argument!.GetType());
            if (serviceProvider.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);
            failures.AddRange(result.Errors);
        }

        if (failures.Count == 0)
        {
            await next();
            return;
        }

        var errors = failures
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray());

        context.Result = new UnprocessableEntityObjectResult(new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "Ошибка проверки запроса",
            Detail = "Исправьте значения указанных полей."
        });
    }
}
