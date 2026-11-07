using EducationSPD.Application.Common;
using EducationSPD.Application.Examples;
using EducationSPD.Contracts;
using EducationSPD.Contracts.Examples;
using Microsoft.AspNetCore.Mvc;

namespace EducationSPD.Web.Features.Examples;

/// <summary>
/// Управление демонстрационными примерами.
/// </summary>
[ApiController]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
public sealed class ExamplesController : ControllerBase
{
    private const int MaxPageSize = 100;
    private readonly IExamplesService service;

    /// <summary>
    /// Создаёт контроллер примеров.
    /// </summary>
    public ExamplesController(IExamplesService service)
    {
        this.service = service;
    }

    /// <summary>
    /// Возвращает страницу примеров.
    /// </summary>
    [HttpGet(ApiRoutes.Examples.ExamplesList, Name = "GetExamples")]
    [ProducesResponseType<ExamplesPageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ExamplesPageResponse>> GetExamples(
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var pagination = new Pagination(Math.Max(offset, 0), Math.Clamp(limit, 1, MaxPageSize));
        var page = await service.GetExamplesAsync(pagination, cancellationToken);

        return Ok(page.ToResponse());
    }

    /// <summary>
    /// Возвращает пример по идентификатору.
    /// </summary>
    [HttpGet(ApiRoutes.Examples.Example, Name = "GetExample")]
    [ProducesResponseType<ExampleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExampleResponse>> GetExample(
        Guid exampleId,
        CancellationToken cancellationToken)
    {
        var example = await service.GetExampleAsync(exampleId, cancellationToken);

        return example is null
            ? NotFound()
            : Ok(example.ToResponse());
    }

    /// <summary>
    /// Создаёт пример.
    /// </summary>
    [HttpPost(ApiRoutes.Examples.ExamplesList, Name = "CreateExample")]
    [ProducesResponseType<ExampleResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ExampleResponse>> CreateExample(
        CreateExampleRequest request,
        CancellationToken cancellationToken)
    {
        var example = await service.CreateExampleAsync(request.ToCommand(), cancellationToken);

        return Created(ApiRoutes.Examples.ForExample(example.Id), example.ToResponse());
    }

    /// <summary>
    /// Изменяет пример.
    /// </summary>
    [HttpPut(ApiRoutes.Examples.Example, Name = "UpdateExample")]
    [ProducesResponseType<ExampleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ExampleResponse>> UpdateExample(
        Guid exampleId,
        UpdateExampleRequest request,
        CancellationToken cancellationToken)
    {
        var example = await service.UpdateExampleAsync(exampleId, request.ToCommand(), cancellationToken);

        return Ok(example.ToResponse());
    }

    /// <summary>
    /// Удаляет пример.
    /// </summary>
    [HttpDelete(ApiRoutes.Examples.Example, Name = "DeleteExample")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExample(
        Guid exampleId,
        CancellationToken cancellationToken)
    {
        await service.DeleteExampleAsync(exampleId, cancellationToken);

        return NoContent();
    }
}
