using EducationSPD.Application.System;
using EducationSPD.Contracts;
using EducationSPD.Contracts.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EducationSPD.Web.Features.System;

/// <summary>
/// Системные проверки HTTP-процесса.
/// </summary>
[ApiController]
[AllowAnonymous]
public sealed class SystemController : ControllerBase
{
    private readonly IServiceReadinessProbe readinessProbe;

    /// <summary>Создаёт системный контроллер.</summary>
    public SystemController(IServiceReadinessProbe readinessProbe)
    {
        this.readinessProbe = readinessProbe;
    }

    /// <summary>
    /// Подтверждает доступность HTTP-процесса.
    /// </summary>
    [HttpGet(ApiRoutes.Health.Status, Name = "Health")]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    public ActionResult<HealthResponse> GetHealth()
    {
        return Ok(new HealthResponse("Healthy"));
    }

    /// <summary>Подтверждает жизнеспособность HTTP-процесса.</summary>
    [HttpGet(ApiRoutes.Health.Live, Name = "Liveness")]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    public ActionResult<HealthResponse> GetLiveness() => Ok(new HealthResponse("Healthy"));

    /// <summary>Проверяет готовность PostgreSQL, миграций и Kafka consumers.</summary>
    [HttpGet(ApiRoutes.Health.Ready, Name = "Readiness")]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<HealthResponse>> GetReadiness(CancellationToken cancellationToken)
    {
        if (await readinessProbe.IsReadyAsync(cancellationToken))
        {
            return Ok(new HealthResponse("Healthy"));
        }

        return StatusCode(StatusCodes.Status503ServiceUnavailable, new HealthResponse("Unhealthy"));
    }
}
