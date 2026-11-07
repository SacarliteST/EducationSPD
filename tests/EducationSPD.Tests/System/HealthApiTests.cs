using System.Net;
using System.Net.Http.Json;
using EducationSPD.Contracts;
using EducationSPD.Contracts.System;
using EducationSPD.Tests.Support;

namespace EducationSPD.Tests.System;

/// <summary>Проверяет базовую доступность API через HTTP-конвейер приложения.</summary>
public sealed class HealthApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task GetHealth_ReturnsHealthyStatus()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(ApiRoutes.Health.Status);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.Equal(new HealthResponse("Healthy"), body);
    }
}
