using EducationSPD.Application.Models;
using EducationSPD.Application.Diagnostics;
using EducationSPD.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EducationSPD.Infrastructure.Models;

internal sealed class ModelRecalculationProcessor : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly TimeSpan pollInterval;
    private readonly ILogger<ModelRecalculationProcessor> logger;

    public ModelRecalculationProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<RecalculationOptions> options,
        ILogger<ModelRecalculationProcessor> logger)
    {
        this.scopeFactory = scopeFactory;
        pollInterval = TimeSpan.FromSeconds(options.Value.PollIntervalSeconds);
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var executor = scope.ServiceProvider.GetRequiredService<IModelRecalculationBatchExecutor>();
                await executor.ExecuteAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                EducationSpdTelemetry.RecordProjectionRecalculation("failed");
                logger.LogError(exception, "Ошибка фонового пересчёта математической модели");
            }

            await Task.Delay(pollInterval, stoppingToken);
        }
    }
}
