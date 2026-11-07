using System.Diagnostics;
using EducationSPD.Application.Diagnostics;
using EducationSPD.Application.Recommendations;
using EducationSPD.Contracts;
using EducationSPD.Contracts.Recommendations;
using EducationSPD.Domain.Episodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EducationSPD.Web.Features.Recommendations;

/// <summary>Ранжирует переданные учебным модулем задания.</summary>
[ApiController]
public sealed class RecommendationsController : ControllerBase
{
    private readonly IRecommendationRankingHandler rankingHandler;
    private readonly ILogger<RecommendationsController> logger;

    /// <summary>Создаёт контроллер рекомендаций.</summary>
    public RecommendationsController(
        IRecommendationRankingHandler rankingHandler,
        ILogger<RecommendationsController> logger)
    {
        this.rankingHandler = rankingHandler;
        this.logger = logger;
    }

    /// <summary>Возвращает персональный порядок всех входных кандидатов.</summary>
    [HttpPost(ApiRoutes.Recommendations.Rank)]
    [ProducesResponseType<RankRecommendationsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RankRecommendationsResponse>> Rank(
        [FromBody] RankRecommendationsRequest request,
        CancellationToken cancellationToken)
    {
        var startedAt = Stopwatch.GetTimestamp();
        var input = new RankRecommendationsInput(
            request.StudentId!.Value,
            request.ModuleKey!,
            request.Candidates!.Select(candidate => new RecommendationCandidateInput(
                candidate.TaskId!.Value,
                candidate.DifficultyLevel!.Value,
                candidate.Skills!.Select(skill => new RecommendationSkillInput(
                    skill.SkillId!,
                    (SkillObservationSource)skill.Source!.Value)).ToArray())).ToArray());
        RecommendationRankingPage result;
        try
        {
            result = await rankingHandler.RankAsync(input, cancellationToken);
        }
        catch
        {
            EducationSpdTelemetry.RecordRecommendation(
                "failed",
                Stopwatch.GetElapsedTime(startedAt),
                input.Candidates.Count);
            throw;
        }
        var duration = Stopwatch.GetElapsedTime(startedAt);
        EducationSpdTelemetry.RecordRecommendation("success", duration, input.Candidates.Count);
        logger.LogInformation(
            "HTTP-ранжирование завершено: {CandidateCount} кандидатов за {ElapsedMilliseconds} мс",
            input.Candidates.Count,
            duration.TotalMilliseconds);
        return Ok(new RankRecommendationsResponse(
            result.ModelVersion,
            result.Items.Select(item => new RankedCandidateResponse(
                item.TaskId,
                item.Rank,
                item.RankScore,
                item.Reasons.Select(reason => new RecommendationReasonResponse(
                    (RecommendationReasonCodeResponse)reason.Code,
                    reason.Contribution,
                    reason.SkillIds)).ToArray())).ToArray()));
    }
}
