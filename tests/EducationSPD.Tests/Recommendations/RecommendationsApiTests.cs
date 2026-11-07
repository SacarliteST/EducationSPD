using System.Net;
using System.Net.Http.Json;
using EducationSPD.Contracts;
using EducationSPD.Contracts.Recommendations;
using EducationSPD.Domain.Skills;
using EducationSPD.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Tests.Recommendations;

public sealed class RecommendationsApiTests(DatabaseApiFixture api)
    : IClassFixture<DatabaseApiFixture>
{
    [Fact]
    public async Task Rank_ReturnsEveryInputCandidateOnceWithModelAndReasons()
    {
        await SeedSkillsAsync();
        var firstTaskId = Guid.NewGuid();
        var secondTaskId = Guid.NewGuid();
        var request = new RankRecommendationsRequest(
            Guid.NewGuid(),
            "sql",
            [
                Candidate(firstTaskId, "join", RecommendationSkillSource.ValidationRule),
                Candidate(secondTaskId, "filter", RecommendationSkillSource.TaskTag),
            ]);
        using var client = CreateAuthorizedClient();

        using var response = await client.PostAsJsonAsync(ApiRoutes.Recommendations.Rank, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<RankRecommendationsResponse>();
        Assert.NotNull(body);
        Assert.Equal(1, body.ModelVersion);
        Assert.Equal(2, body.Items.Count);
        Assert.Equal(
            new[] { firstTaskId, secondTaskId }.Order(),
            body.Items.Select(item => item.TaskId).Order());
        Assert.Equal([1, 2], body.Items.Select(item => item.Rank));
        Assert.All(body.Items, item => Assert.Equal(4, item.Reasons.Count));
    }

    [Fact]
    public async Task Rank_UnknownAndDeletedSkillsReturnStructuredValidationProblem()
    {
        await SeedSkillsAsync();
        var request = new RankRecommendationsRequest(
            Guid.NewGuid(),
            "sql",
            [
                new RecommendationCandidateRequest(
                    Guid.NewGuid(),
                    2,
                    [
                        new RecommendationSkillRequest("missing", RecommendationSkillSource.ValidationRule),
                        new RecommendationSkillRequest("removed", RecommendationSkillSource.TaskTag),
                    ])
            ]);
        using var client = CreateAuthorizedClient();

        using var response = await client.PostAsJsonAsync(ApiRoutes.Recommendations.Rank, request);
        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsContract>();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.NotNull(body);
        Assert.Contains("Candidates.Skills", body.Errors.Keys);
        Assert.Contains("Candidates.Skills.Deleted", body.Errors.Keys);
    }

    [Fact]
    public async Task Rank_InvalidCandidateShapeReturnsUnprocessableEntity()
    {
        var duplicateTaskId = Guid.NewGuid();
        var request = new RankRecommendationsRequest(
            Guid.NewGuid(),
            "sql",
            [
                Candidate(duplicateTaskId, "join", RecommendationSkillSource.ValidationRule),
                Candidate(duplicateTaskId, "join", RecommendationSkillSource.ValidationRule),
            ]);
        using var client = CreateAuthorizedClient();

        using var response = await client.PostAsJsonAsync(ApiRoutes.Recommendations.Rank, request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    private HttpClient CreateAuthorizedClient()
    {
        var client = api.CreateClient();
        client.DefaultRequestHeaders.Add("X-Service-Key", DatabaseApiFixture.ServiceKey);
        return client;
    }

    private async Task SeedSkillsAsync()
    {
        await using var dbContext = api.CreateDbContext();
        if (await dbContext.Skills.AnyAsync(skill => skill.ModuleKey == "sql"))
        {
            return;
        }

        dbContext.Skills.Add(CreateSkill("join", false));
        dbContext.Skills.Add(CreateSkill("filter", false));
        dbContext.Skills.Add(CreateSkill("removed", true));
        await dbContext.SaveChangesAsync();
    }

    private static RecommendationCandidateRequest Candidate(
        Guid taskId,
        string skillId,
        RecommendationSkillSource source) =>
        new(taskId, 2, [new RecommendationSkillRequest(skillId, source)]);

    private static Skill CreateSkill(string skillId, bool isDeleted) =>
        Skill.Create(
            Guid.NewGuid(),
            "sql",
            skillId,
            skillId,
            null,
            SkillOrigin.System,
            isDeleted,
            DateTimeOffset.UtcNow,
            Guid.NewGuid());

    private sealed record ValidationProblemDetailsContract(
        IReadOnlyDictionary<string, string[]> Errors);
}
