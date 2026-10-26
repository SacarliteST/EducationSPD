using EducationSPD.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace EducationSPD.Tests.Support;

/// <summary>Поднимает API поверх реальной PostgreSQL в контейнере.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public async Task InitializeAsync()
    {
        await postgres.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Default", postgres.GetConnectionString());
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await postgres.DisposeAsync();
    }
}
