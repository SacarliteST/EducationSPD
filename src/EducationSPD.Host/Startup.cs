using EducationSPD.Application;
using EducationSPD.Infrastructure;
using EducationSPD.Infrastructure.Configuration;
using EducationSPD.Infrastructure.Persistence.Migrations;
using EducationSPD.Web;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

namespace EducationSPD.Host;

internal static class Startup
{
    private const string FrontendCorsPolicy = "Frontend";

    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddHttpLogging(options =>
        {
            options.LoggingFields = HttpLoggingFields.RequestMethod
                | HttpLoggingFields.RequestPath
                | HttpLoggingFields.ResponseStatusCode
                | HttpLoggingFields.Duration;
        });

        services.AddApplication();
        services.AddInfrastructure(builder.Configuration);
        services.AddWeb(builder.Configuration);
        services.AddOpenApi();
        services.AddCors(options =>
        {
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>();

            options.AddPolicy(FrontendCorsPolicy, policy =>
            {
                policy.WithOrigins(allowedOrigins is { Length: > 0 }
                        ? allowedOrigins
                        : ["http://localhost:5173", "https://localhost:5173"])
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }

    public static async Task ConfigureAppAsync(WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseHttpLogging();

        await ApplyMigrationsAsync(app);

        app.UseCors(FrontendCorsPolicy);

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.MapControllers();
    }

    private static async Task ApplyMigrationsAsync(WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<PostgresOptions>>().Value;
        if (!app.Environment.IsDevelopment() && !options.ApplyMigrationsOnStartup)
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var migrationManager = scope.ServiceProvider.GetRequiredService<IDatabaseMigrationManager>();
        await migrationManager.MigrateAsync();
    }
}
