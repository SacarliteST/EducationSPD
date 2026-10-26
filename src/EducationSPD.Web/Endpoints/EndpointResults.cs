using EducationSPD.Application.Examples;
using FluentValidation;

namespace EducationSPD.Web.Endpoints;

/// <summary>Общие обёртки эндпоинтов: валидация запроса и перевод доменных исключений в HTTP-ответы.</summary>
internal static class EndpointResults
{
    /// <summary>Проверяет запрос валидатором.</summary>
    /// <typeparam name="TRequest">Тип запроса.</typeparam>
    /// <param name="validator">Валидатор запроса.</param>
    /// <param name="request">Проверяемый запрос.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Ответ 400 с описанием ошибок или <see langword="null"/>, если запрос корректен.</returns>
    public static async Task<IResult?> ValidateAsync<TRequest>(
        IValidator<TRequest> validator,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsValid)
        {
            return null;
        }

        var errors = validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        return Results.ValidationProblem(errors);
    }

    /// <summary>Выполняет команду без результата и переводит исключения в HTTP-ответы (204 при успехе).</summary>
    /// <param name="command">Выполняемая команда.</param>
    /// <returns>Ответ HTTP.</returns>
    public static async Task<IResult> ExecuteCommandAsync(Func<Task> command)
    {
        return await ExecuteCommandAsync(async () =>
        {
            await command();
            return Results.NoContent();
        });
    }

    /// <summary>Выполняет команду и переводит исключения в HTTP-ответы.</summary>
    /// <param name="command">Выполняемая команда, возвращающая ответ.</param>
    /// <returns>Ответ команды или ответ об ошибке.</returns>
    public static async Task<IResult> ExecuteCommandAsync(Func<Task<IResult>> command)
    {
        try
        {
            return await command();
        }
        catch (ExampleNotFoundException exception)
        {
            return Results.Problem(exception.Message, statusCode: StatusCodes.Status404NotFound);
        }
    }
}
