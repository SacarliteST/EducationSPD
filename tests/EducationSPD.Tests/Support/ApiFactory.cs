using EducationSPD.Host;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EducationSPD.Tests.Support;

/// <summary>Поднимает HTTP-конвейер приложения для системных тестов.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.UseSetting("ConnectionStrings:Postgres", "Host=unused;Database=unused;Username=unused;Password=unused");
        builder.UseSetting("ServiceAuthentication:ServiceKey", "education-spd-system-test-key");
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "false");
    }
}
