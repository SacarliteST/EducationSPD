using System.Text.Json;
using Confluent.Kafka;
using EducationSPD.Application.Inbox;
using EducationSPD.Domain.Inbox;
using EducationSPD.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EducationSPD.Infrastructure.Kafka;

internal sealed class SkillCatalogKafkaConsumer(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaOptions> options,
    ILogger<SkillCatalogKafkaConsumer> logger) : BackgroundService
{
    private readonly KafkaOptions options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = options.BootstrapServers,
            GroupId = options.ConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(options.SkillCatalogTopic);
        logger.LogInformation(
            "Запущено чтение каталога навыков из Kafka-топика {Topic}",
            options.SkillCatalogTopic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? result;
                try
                {
                    result = consumer.Consume(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (ConsumeException exception)
                {
                    logger.LogWarning(
                        exception,
                        "Не удалось прочитать сообщение каталога навыков из Kafka");
                    await DelayBeforeRetryAsync(stoppingToken);
                    continue;
                }

                if (result?.Message is null)
                {
                    continue;
                }

                try
                {
                    if (await StoreAsync(result, stoppingToken))
                    {
                        consumer.Commit(result);
                    }
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    logger.LogError(
                        exception,
                        "Не удалось надёжно сохранить сообщение каталога навыков");
                    consumer.Seek(result.TopicPartitionOffset);
                    await DelayBeforeRetryAsync(stoppingToken);
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task<bool> StoreAsync(
        ConsumeResult<string, string> result,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(result.Message.Value)
            || string.IsNullOrWhiteSpace(result.Message.Key))
        {
            logger.LogError(
                "Отклонено сообщение каталога навыков без key или payload: offset {Offset}",
                result.Offset.Value);
            return true;
        }

        KafkaInboxEnvelope envelope;
        try
        {
            envelope = KafkaInboxEnvelopeReader.Read(result.Message.Value);
        }
        catch (JsonException exception)
        {
            logger.LogError(
                exception,
                "Отклонено сообщение каталога навыков без корректного envelope: offset {Offset}",
                result.Offset.Value);
            return true;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var inboxStore = scope.ServiceProvider.GetRequiredService<IInboxStore>();
        var writeResult = await inboxStore.StoreAsync(
            new InboxMessageInput(
                envelope.EventId,
                InboxMessageType.SkillCatalogChanged,
                envelope.SchemaVersion,
                result.Message.Key,
                result.Message.Value),
            cancellationToken);

        if (writeResult == InboxWriteResult.Conflict)
        {
            logger.LogError(
                "Обнаружен конфликт payload для события каталога навыков {EventId}",
                envelope.EventId);
        }

        return true;
    }

    private Task DelayBeforeRetryAsync(CancellationToken cancellationToken)
    {
        return Task.Delay(options.InitialRetryDelayMilliseconds, cancellationToken);
    }
}
