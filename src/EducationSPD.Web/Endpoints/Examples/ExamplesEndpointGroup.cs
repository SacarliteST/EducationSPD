using EducationSPD.Application.Common;
using EducationSPD.Application.Examples;
using EducationSPD.Contracts;
using EducationSPD.Contracts.Examples;
using FluentValidation;

namespace EducationSPD.Web.Endpoints;

/// <summary>Маршруты раздела «примеры». Образец для новых endpoint-групп.</summary>
internal static class ExamplesEndpointGroup
{
    private const int MaxPageSize = 100;

    /// <summary>Регистрирует маршруты раздела.</summary>
    /// <param name="app">Построитель маршрутов.</param>
    /// <returns>Тот же построитель маршрутов.</returns>
    public static IEndpointRouteBuilder MapExamplesEndpointGroup(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Examples.ExamplesList, async (
                IExamplesService service,
                CancellationToken cancellationToken,
                int offset = 0,
                int limit = 50) =>
            {
                var pagination = new Pagination(Math.Max(offset, 0), Math.Clamp(limit, 1, MaxPageSize));
                var page = await service.GetExamplesAsync(pagination, cancellationToken);
                return Results.Ok(page.ToResponse());
            })
            .WithTags("Examples")
            .WithName("GetExamples")
            .WithSummary("Список примеров")
            .WithDescription("Возвращает страницу примеров. Параметры offset и limit (1–100, по умолчанию 50).")
            .Produces<ExamplesPageResponse>();

        app.MapGet(ApiRoutes.Examples.Example, async (
                Guid exampleId,
                IExamplesService service,
                CancellationToken cancellationToken) =>
            {
                var example = await service.GetExampleAsync(exampleId, cancellationToken);
                return example is null ? Results.NotFound() : Results.Ok(example.ToResponse());
            })
            .WithTags("Examples")
            .WithName("GetExample")
            .WithSummary("Пример по идентификатору")
            .WithDescription("Возвращает пример или 404, если он не найден.")
            .Produces<ExampleResponse>()
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost(ApiRoutes.Examples.ExamplesList, async (
                CreateExampleRequest request,
                IValidator<CreateExampleRequest> validator,
                IExamplesService service,
                CancellationToken cancellationToken) =>
            {
                var validation = await EndpointResults.ValidateAsync(validator, request, cancellationToken);
                if (validation is not null)
                {
                    return validation;
                }

                var example = await service.CreateExampleAsync(request.ToCommand(), cancellationToken);
                return Results.Created(ApiRoutes.Examples.ForExample(example.Id), example.ToResponse());
            })
            .WithTags("Examples")
            .WithName("CreateExample")
            .WithSummary("Создание примера")
            .WithDescription("Создаёт новый пример.")
            .Produces<ExampleResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        app.MapPut(ApiRoutes.Examples.Example, async (
                Guid exampleId,
                UpdateExampleRequest request,
                IValidator<UpdateExampleRequest> validator,
                IExamplesService service,
                CancellationToken cancellationToken) =>
            await EndpointResults.ExecuteCommandAsync(async () =>
            {
                var validation = await EndpointResults.ValidateAsync(validator, request, cancellationToken);
                if (validation is not null)
                {
                    return validation;
                }

                var example = await service.UpdateExampleAsync(exampleId, request.ToCommand(), cancellationToken);
                return Results.Ok(example.ToResponse());
            }))
            .WithTags("Examples")
            .WithName("UpdateExample")
            .WithSummary("Изменение примера")
            .WithDescription("Изменяет название и описание примера.")
            .Produces<ExampleResponse>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        app.MapDelete(ApiRoutes.Examples.Example, async (
                Guid exampleId,
                IExamplesService service,
                CancellationToken cancellationToken) =>
            await EndpointResults.ExecuteCommandAsync(() => service.DeleteExampleAsync(exampleId, cancellationToken)))
            .WithTags("Examples")
            .WithName("DeleteExample")
            .WithSummary("Удаление примера")
            .WithDescription("Удаляет пример.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
