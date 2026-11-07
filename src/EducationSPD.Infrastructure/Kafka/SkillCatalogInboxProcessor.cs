using System.Text.Json;
using EducationSPD.Application.Inbox;
using EducationSPD.Application.Skills;
using EducationSPD.Domain.Inbox;
using EducationSPD.Domain.Skills;
using EducationSPD.Kafka.Contracts;
using EducationSPD.Kafka.Contracts.V1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EducationSPD.Infrastructure.Kafka;

internal sealed class SkillCatalogInboxProcessor(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<SkillCatalogInboxProcessor> logger) : BackgroundService
{
    private static readonly TimeSpan EmptyBatchDelay = TimeSpan.FromMilliseconds(250);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processedCount = await ProcessBatchAsync(stoppingToken);
                if (processedCount == 0)
                {
                    await Task.Delay(EmptyBatchDelay, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Ошибка применения каталога навыков из inbox");
                await Task.Delay(EmptyBatchDelay, stoppingToken);
            }
        }
    }

    private async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var executor = scope.ServiceProvider.GetRequiredService<IInboxBatchExecutor>();
        var handler = scope.ServiceProvider.GetRequiredService<ISkillCatalogChangeHandler>();
        var pendingCatalogNotifier = scope.ServiceProvider.GetRequiredService<IPendingCatalogNotifier>();

        return await executor.ExecuteReadyBatchAsync(
            InboxMessageType.SkillCatalogChanged,
            async (messages, batchCancellationToken) =>
            {
                foreach (var message in messages)
                {
                    await ApplyAsync(
                        message,
                        handler,
                        pendingCatalogNotifier,
                        batchCancellationToken);
                }
            },
            cancellationToken);
    }

    private async Task ApplyAsync(
        InboxMessage inboxMessage,
        ISkillCatalogChangeHandler handler,
        IPendingCatalogNotifier pendingCatalogNotifier,
        CancellationToken cancellationToken)
    {
        if (inboxMessage.SchemaVersion != KafkaSchemaVersions.Version1)
        {
            inboxMessage.MarkFailed("UnsupportedSchemaVersion", timeProvider.GetUtcNow());
            return;
        }

        SkillCatalogChanged message;
        try
        {
            message = KafkaContractJson.Deserialize<SkillCatalogChanged>(inboxMessage.Payload);
        }
        catch (Exception exception) when (exception is JsonException or KafkaContractValidationException)
        {
            logger.LogWarning(
                "Отклонено некорректное событие каталога навыков {EventId}",
                inboxMessage.EventId);
            inboxMessage.MarkFailed("InvalidContract", timeProvider.GetUtcNow());
            return;
        }

        await handler.ApplyAsync(
            new SkillCatalogChange(
                message.EventId,
                message.ModuleKey,
                message.SkillId,
                message.Name,
                message.Description,
                MapOrigin(message.Origin),
                message.IsDeleted,
                message.OccurredAt),
            cancellationToken);
        await pendingCatalogNotifier.ResumeAsync(
            message.ModuleKey,
            message.SkillId,
            cancellationToken);
        inboxMessage.MarkApplied(timeProvider.GetUtcNow());
    }

    private static EducationSPD.Domain.Skills.SkillOrigin MapOrigin(
        EducationSPD.Kafka.Contracts.V1.SkillOrigin origin)
    {
        return origin switch
        {
            EducationSPD.Kafka.Contracts.V1.SkillOrigin.System =>
                EducationSPD.Domain.Skills.SkillOrigin.System,
            EducationSPD.Kafka.Contracts.V1.SkillOrigin.TeacherDefined =>
                EducationSPD.Domain.Skills.SkillOrigin.TeacherDefined,
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };
    }
}
