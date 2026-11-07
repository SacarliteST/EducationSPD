using EducationSPD.Application.Recommendations;
using EducationSPD.Contracts;
using EducationSPD.Contracts.Recommendations;
using EducationSPD.Domain.Episodes;
using Microsoft.AspNetCore.Mvc;

namespace EducationSPD.Web.Features.Recommendations;

/// <summary>Ранжирует переданные учебным модулем задания.</summary>
[ApiController]
public sealed class RecommendationsController : ControllerBase
{
    private readonly IRecommendationRankingHandler rankingHandler;

    /// <summary>Создаёт контроллер рекомендаций.</summary>
    public RecommendationsController(IRecommendationRankingHandler rankingHandler)
    {
        this.rankingHandler = rankingHandler;
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
        var input = new RankRecommendationsInput(
            request.StudentId!.Value,
            request.ModuleKey!,
            request.Candidates!.Select(candidate => new RecommendationCandidateInput(
                candidate.TaskId!.Value,
                candidate.DifficultyLevel!.Value,
                candidate.Skills!.Select(skill => new RecommendationSkillInput(
                    skill.SkillId!,
                    (SkillObservationSource)skill.Source!.Value)).ToArray())).ToArray());
        var result = await rankingHandler.RankAsync(input, cancellationToken);
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
