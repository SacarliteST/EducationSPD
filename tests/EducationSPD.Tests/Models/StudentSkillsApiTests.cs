using System.Net;
using System.Net.Http.Json;
using EducationSPD.Contracts;
using EducationSPD.Contracts.Skills;
using EducationSPD.Domain.Skills;
using EducationSPD.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Tests.Models;

public sealed class StudentSkillsApiTests(DatabaseApiFixture api)
    : IClassFixture<DatabaseApiFixture>
{
    [Fact]
    public async Task Get_ReturnsActiveCatalogAndInitialMasteryForUnobservedSkill()
    {
        await SeedSkillsAsync();
        using var client = CreateAuthorizedClient();
        var studentId = Guid.NewGuid();

        using var response = await client.GetAsync(
            ApiRoutes.StudentSkills.List
                .Replace("{moduleKey}", "sql", StringComparison.Ordinal)
                .Replace("{studentId:guid}", studentId.ToString(), StringComparison.Ordinal));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<StudentSkillsResponse>();
        Assert.NotNull(body);
        Assert.Equal(1, body.Count);
        Assert.Equal(1, body.ModelVersion);
        var skill = Assert.Single(body.Items);
        Assert.Equal("join", skill.SkillId);
        Assert.Equal(StudentSkillOrigin.System, skill.Origin);
        Assert.Equal(0.2m, skill.MasteryProbability);
        Assert.Equal(0, skill.ObservationsCount);
        Assert.Null(skill.LastObservedAt);
    }

    [Fact]
    public async Task Get_IncludeDeletedAndPaginationValidationWork()
    {
        await SeedSkillsAsync();
        using var client = CreateAuthorizedClient();
        var route = ApiRoutes.StudentSkills.List
            .Replace("{moduleKey}", "sql", StringComparison.Ordinal)
            .Replace("{studentId:guid}", Guid.NewGuid().ToString(), StringComparison.Ordinal);

        using var includedResponse = await client.GetAsync($"{route}?includeDeleted=true");
        var included = await includedResponse.Content.ReadFromJsonAsync<StudentSkillsResponse>();
        using var invalidResponse = await client.GetAsync($"{route}?limit=501");

        Assert.Equal(HttpStatusCode.OK, includedResponse.StatusCode);
        Assert.NotNull(included);
        Assert.Equal(2, included.Count);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalidResponse.StatusCode);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("wrong-key", HttpStatusCode.Forbidden)]
    public async Task Get_RequiresValidServiceKey(string? serviceKey, HttpStatusCode expectedStatus)
    {
        using var client = api.CreateClient();
        if (serviceKey is not null)
        {
            client.DefaultRequestHeaders.Add("X-Service-Key", serviceKey);
        }

        var route = ApiRoutes.StudentSkills.List
            .Replace("{moduleKey}", "sql", StringComparison.Ordinal)
            .Replace("{studentId:guid}", Guid.NewGuid().ToString(), StringComparison.Ordinal);
        using var response = await client.GetAsync(route);

        Assert.Equal(expectedStatus, response.StatusCode);
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
        dbContext.Skills.Add(CreateSkill("removed", true));
        await dbContext.SaveChangesAsync();
    }

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
}
