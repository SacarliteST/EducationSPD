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
    /// <summary>
    /// Подтверждает доступность HTTP-процесса.
    /// </summary>
    [HttpGet(ApiRoutes.Health.Status, Name = "Health")]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    public ActionResult<HealthResponse> GetHealth()
    {
        return Ok(new HealthResponse("Healthy"));
    }
}
