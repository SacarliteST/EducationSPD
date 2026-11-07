using System.Net;
using System.Net.Http.Json;
using EducationSPD.Contracts;
using EducationSPD.Contracts.Examples;
using EducationSPD.Tests.Support;
using EducationSPD.Web.Security;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace EducationSPD.Tests.Examples;

public sealed class ExamplesApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory factory;
    private readonly HttpClient client;

    public ExamplesApiTests(ApiFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(ServiceKeyAuthorizationFilter.HeaderName, ApiFactory.ServiceKey);
    }

    [Fact]
    public async Task Crud_flow_works_against_real_database()
    {
        var created = await client.PostAsJsonAsync(ApiRoutes.Examples.ExamplesList, new CreateExampleRequest("Первый", "описание"));
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        var example = (await created.Content.ReadFromJsonAsync<ExampleResponse>())!;

        var updated = await client.PutAsJsonAsync(ApiRoutes.Examples.ForExample(example.Id), new UpdateExampleRequest("Второй", null));
        updated.StatusCode.ShouldBe(HttpStatusCode.OK);

        var fetched = (await client.GetFromJsonAsync<ExampleResponse>(ApiRoutes.Examples.ForExample(example.Id)))!;
        fetched.Name.ShouldBe("Второй");

        var page = (await client.GetFromJsonAsync<ExamplesPageResponse>(ApiRoutes.Examples.ExamplesList))!;
        page.Items.ShouldContain(item => item.Id == example.Id);

        (await client.DeleteAsync(ApiRoutes.Examples.ForExample(example.Id))).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await client.GetAsync(ApiRoutes.Examples.ForExample(example.Id))).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_with_empty_name_returns_validation_problem()
    {
        var response = await client.PostAsJsonAsync(ApiRoutes.Examples.ExamplesList, new CreateExampleRequest("", null));
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("Name");
    }

    [Fact]
    public async Task Update_unknown_example_returns_not_found()
    {
        var response = await client.PutAsJsonAsync(ApiRoutes.Examples.ForExample(Guid.NewGuid()), new UpdateExampleRequest("x", null));
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Request_without_service_key_returns_unauthorized()
    {
        using var anonymousClient = factory.CreateClient();

        var response = await anonymousClient.GetAsync(ApiRoutes.Examples.ExamplesList);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Request_with_invalid_service_key_returns_forbidden()
    {
        using var unauthorizedClient = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, ApiRoutes.Examples.ExamplesList);
        request.Headers.Add(ServiceKeyAuthorizationFilter.HeaderName, "invalid-key");

        var response = await unauthorizedClient.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Health_does_not_require_service_key()
    {
        using var anonymousClient = factory.CreateClient();

        var response = await anonymousClient.GetAsync(ApiRoutes.Health.Status);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
