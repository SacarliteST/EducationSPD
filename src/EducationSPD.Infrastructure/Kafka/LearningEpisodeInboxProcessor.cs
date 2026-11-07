using System.Text.Json;
using EducationSPD.Application.Episodes;
using EducationSPD.Application.Inbox;
using EducationSPD.Domain.Episodes;
using EducationSPD.Domain.Inbox;
using EducationSPD.Infrastructure.Configuration;
using EducationSPD.Kafka.Contracts;
using EducationSPD.Kafka.Contracts.V1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EducationSPD.Infrastructure.Kafka;

internal sealed class LearningEpisodeInboxProcessor(
    IServiceScopeFactory scopeFactory,
    IOptions<InboxOptions> options,
    IOptions<KafkaOptions> kafkaOptions,
    TimeProvider timeProvider,
    ILogger<LearningEpisodeInboxProcessor> logger) : BackgroundService
{
    private static readonly TimeSpan EmptyBatchDelay = TimeSpan.FromMilliseconds(250);
    private readonly InboxOptions options = options.Value;
    private readonly KafkaOptions kafkaOptions = kafkaOptions.Value;

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
                logger.LogError(exception, "Ошибка применения учебных эпизодов из inbox");
                await Task.Delay(EmptyBatchDelay, stoppingToken);
            }
        }
    }

    private async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var executor = scope.ServiceProvider.GetRequiredService<IInboxBatchExecutor>();
        var handler = scope.ServiceProvider.GetRequiredService<ILearningEpisodeFinalizationHandler>();

        return await executor.ExecuteReadyBatchAsync(
            InboxMessageType.LearningEpisodeFinalized,
            async (messages, batchCancellationToken) =>
            {
                foreach (var message in messages)
                {
                    await ApplyAsync(message, handler, batchCancellationToken);
                }
            },
            cancellationToken);
    }

    private async Task ApplyAsync(
        InboxMessage inboxMessage,
        ILearningEpisodeFinalizationHandler handler,
        CancellationToken cancellationToken)
    {
        if (inboxMessage.SchemaVersion != KafkaSchemaVersions.Version1)
        {
            inboxMessage.MarkFailed("UnsupportedSchemaVersion", timeProvider.GetUtcNow());
            return;
        }

        LearningEpisodeFinalized message;
        try
        {
            message = KafkaContractJson.Deserialize<LearningEpisodeFinalized>(inboxMessage.Payload);
        }
        catch (Exception exception) when (exception is JsonException or KafkaContractValidationException)
        {
            logger.LogWarning(
                "Отклонено некорректное событие учебного эпизода {EventId}",
                inboxMessage.EventId);
            inboxMessage.MarkFailed("InvalidContract", timeProvider.GetUtcNow());
            return;
        }

        var result = await handler.ApplyAsync(Map(message), cancellationToken);
        switch (result)
        {
            case LearningEpisodeApplyResult.Applied:
            case LearningEpisodeApplyResult.Duplicate:
                inboxMessage.MarkApplied(timeProvider.GetUtcNow());
                break;
            case LearningEpisodeApplyResult.Conflict:
                inboxMessage.MarkFailed("EpisodeConflict", timeProvider.GetUtcNow());
                break;
            case LearningEpisodeApplyResult.MissingCatalog:
                if (inboxMessage.RetryCount >= kafkaOptions.MaxRetryAttempts - 1)
                {
                    inboxMessage.MarkFailed("CatalogRetryLimitExceeded", timeProvider.GetUtcNow());
                    logger.LogWarning(
                        "Эпизод {EventId} отклонён после исчерпания повторов ожидания каталога",
                        inboxMessage.EventId);
                }
                else
                {
                    inboxMessage.WaitForCatalog(
                        timeProvider.GetUtcNow().AddSeconds(options.PendingCatalogRetryIntervalSeconds));
                    logger.LogInformation(
                        "Эпизод {EventId} ожидает обновления каталога навыков",
                        inboxMessage.EventId);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result));
        }
    }

    private static LearningEpisodeFinalization Map(LearningEpisodeFinalized message)
    {
        return new LearningEpisodeFinalization(
            message.EventId,
            message.EpisodeId,
            message.StudentId,
            message.ModuleKey,
            message.TaskId,
            message.FinalizedAt,
            message.SkillResults
                .Select(result => new LearningEpisodeSkillResult(
                    result.SkillId,
                    MapSource(result.Source),
                    MapOutcome(result.Outcome)))
                .ToArray());
    }

    private static SkillObservationSource MapSource(SkillResultSource source)
    {
        return source switch
        {
            SkillResultSource.ValidationRule => SkillObservationSource.ValidationRule,
            SkillResultSource.TaskTag => SkillObservationSource.TaskTag,
            _ => throw new ArgumentOutOfRangeException(nameof(source))
        };
    }

    private static SkillObservationOutcome MapOutcome(SkillOutcome outcome)
    {
        return outcome switch
        {
            SkillOutcome.Succeeded => SkillObservationOutcome.Succeeded,
            SkillOutcome.Failed => SkillObservationOutcome.Failed,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome))
        };
    }
}
