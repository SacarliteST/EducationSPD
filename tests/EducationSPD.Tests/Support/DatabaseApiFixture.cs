using EducationSPD.Host;
using EducationSPD.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace EducationSPD.Tests.Support;

public sealed class DatabaseApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
    private ConfiguredApiFactory? factory;

    public const string ServiceKey = "education-spd-database-test-key";

    public async Task InitializeAsync()
    {
        await postgres.StartAsync();
        factory = new ConfiguredApiFactory(postgres.GetConnectionString());
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("health");
        response.EnsureSuccessStatusCode();
    }

    public HttpClient CreateClient() => factory!.CreateClient();

    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .Options;
        return new AppDbContext(options);
    }

    public async Task DisposeAsync()
    {
        factory?.Dispose();
        await postgres.DisposeAsync();
    }

    private sealed class ConfiguredApiFactory(string connectionString)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("ConnectionStrings:Postgres", connectionString);
            builder.UseSetting("ServiceAuthentication:ServiceKey", ServiceKey);
            builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
            builder.ConfigureServices(services =>
            {
                var backgroundServices = services
                    .Where(descriptor => descriptor.ServiceType == typeof(IHostedService))
                    .ToArray();
                foreach (var backgroundService in backgroundServices)
                {
                    services.Remove(backgroundService);
                }
            });
        }
    }
}
