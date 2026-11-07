using System.Text.Json.Serialization;
using EducationSPD.Application.Examples;
using EducationSPD.Contracts.Examples;
using EducationSPD.Infrastructure;
using EducationSPD.Infrastructure.Configuration;
using EducationSPD.Infrastructure.Persistence;
using EducationSPD.Web.Common;
using EducationSPD.Web.Features.Examples.Validators;
using EducationSPD.Web.Security;
using FluentValidation;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
var allowedOrigins = configuredOrigins is { Length: > 0 }
    ? configuredOrigins
    : ["http://localhost:5173", "https://localhost:5173"];
const string frontendCorsPolicy = "Frontend";

// Каждый запрос — одна Information-строка (метод/путь/статус/длительность).
// Без заголовков и тел запроса/ответа, чтобы в лог не попали токены и персональные данные.
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.Duration;
});

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IExamplesService, ExamplesService>();
builder.Services.AddScoped<IValidator<CreateExampleRequest>, CreateExampleRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateExampleRequest>, UpdateExampleRequestValidator>();
builder.Services.AddAppInfrastructure(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddScoped<FluentValidationActionFilter>();
builder.Services.AddScoped<ServiceKeyAuthorizationFilter>();
builder.Services.AddOptions<ServiceAuthenticationOptions>()
    .BindConfiguration(ServiceAuthenticationOptions.SectionName)
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ServiceKey),
        $"{ServiceAuthenticationOptions.SectionName}:ServiceKey обязателен.")
    .ValidateOnStart();

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<FluentValidationActionFilter>();
        options.Filters.Add<ServiceKeyAuthorizationFilter>();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = ApiValidationProblemFactory.Create;
    })
    .AddJsonOptions(options =>
    {
        // Числа принимаются только числами: иначе OpenAPI документирует числовые поля
        // как union [number, string], а сгенерированные клиенты получают number | string.
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
    });

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Первым в конвейере — ловит исключения из всего, что ниже.
app.UseExceptionHandler();
app.UseHttpLogging();

// В Development миграции применяются всегда; вне него — только при
// Database:ApplyMigrationsOnStartup=true (по умолчанию false).
var postgresOptions = app.Services.GetRequiredService<IOptions<PostgresOptions>>().Value;
var applyMigrationsOnStartup = app.Environment.IsDevelopment()
    || postgresOptions.ApplyMigrationsOnStartup;

if (applyMigrationsOnStartup)
{
    await using var migrationScope = app.Services.CreateAsyncScope();
    var dbContext = migrationScope.ServiceProvider.GetRequiredService<AppDbContext>();

    // MigrateAsync берёт advisory-lock на __EFMigrationsHistory — безопасно при нескольких репликах.
    await dbContext.Database.MigrateAsync();
}

app.UseCors(frontendCorsPolicy);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

app.Run();

/// <summary>
/// Точка входа API, используемая приложением и интеграционными тестами.
/// </summary>
public partial class Program;
