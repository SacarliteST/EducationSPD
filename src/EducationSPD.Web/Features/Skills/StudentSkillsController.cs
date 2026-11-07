using EducationSPD.Application.Models;
using EducationSPD.Contracts;
using EducationSPD.Contracts.Skills;
using EducationSPD.Domain.Skills;
using Microsoft.AspNetCore.Mvc;

namespace EducationSPD.Web.Features.Skills;

/// <summary>Предоставляет доверенным сервисам состояния навыков студентов.</summary>
[ApiController]
public sealed class StudentSkillsController : ControllerBase
{
    private readonly IStudentSkillsQueryHandler queryHandler;

    /// <summary>Создаёт контроллер чтения состояния навыков.</summary>
    public StudentSkillsController(IStudentSkillsQueryHandler queryHandler)
    {
        this.queryHandler = queryHandler;
    }

    /// <summary>Возвращает страницу навыков студента для активной версии модели.</summary>
    [HttpGet(ApiRoutes.StudentSkills.List)]
    [ProducesResponseType<StudentSkillsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StudentSkillsResponse>> Get(
        [FromRoute] string moduleKey,
        [FromRoute] Guid studentId,
        [FromQuery] GetStudentSkillsRequest request,
        CancellationToken cancellationToken)
    {
        var routeError = ValidateRoute(moduleKey, studentId);
        if (routeError is not null)
        {
            return UnprocessableEntity(routeError);
        }

        var page = await queryHandler.GetAsync(
            moduleKey,
            studentId,
            request.IncludeDeleted,
            request.Offset,
            request.Limit,
            cancellationToken);
        var response = new StudentSkillsResponse(
            page.Count,
            page.Offset,
            page.Limit,
            page.ModelVersion,
            page.Items.Select(MapItem).ToArray());
        return Ok(response);
    }

    private static StudentSkillResponse MapItem(StudentSkillReadItem item) =>
        new(
            item.SkillId,
            item.Name,
            item.Description,
            (StudentSkillOrigin)item.Origin,
            item.IsDeleted,
            item.MasteryProbability,
            item.ObservationsCount,
            item.LastObservedAt);

    private static ValidationProblemDetails? ValidateRoute(string moduleKey, Guid studentId)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(moduleKey)
            || moduleKey.Length > Skill.ModuleKeyMaxLength
            || moduleKey.Any(character => character is not (>= 'a' and <= 'z')
                && character is not (>= '0' and <= '9')
                && character != '-'))
        {
            errors[nameof(moduleKey)] = ["Ключ модуля заполнен некорректно."];
        }

        if (studentId == Guid.Empty)
        {
            errors[nameof(studentId)] = ["Идентификатор студента не может быть пустым."];
        }

        return errors.Count == 0
            ? null
            : new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Ошибка проверки запроса",
                Detail = "Исправьте значения указанных полей.",
            };
    }
}
