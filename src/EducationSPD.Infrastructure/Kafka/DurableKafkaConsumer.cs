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

internal abstract class DurableKafkaConsumer(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaOptions> options,
    ILogger logger,
    string topic,
    InboxMessageType messageType) : BackgroundService
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
        consumer.Subscribe(topic);
        logger.LogInformation("Запущено чтение Kafka-топика {Topic}", topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await ConsumeAsync(consumer, stoppingToken);
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
                        "Не удалось надёжно сохранить Kafka-сообщение типа {MessageType}",
                        messageType);
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

    private async Task<ConsumeResult<string, string>?> ConsumeAsync(
        IConsumer<string, string> consumer,
        CancellationToken cancellationToken)
    {
        try
        {
            return consumer.Consume(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return null;
        }
        catch (ConsumeException exception)
        {
            logger.LogWarning(exception, "Не удалось прочитать Kafka-топик {Topic}", topic);
            await DelayBeforeRetryAsync(cancellationToken);
            return null;
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
                "Отклонено Kafka-сообщение типа {MessageType} без key или payload: offset {Offset}",
                messageType,
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
                "Отклонено Kafka-сообщение типа {MessageType} без корректного envelope: offset {Offset}",
                messageType,
                result.Offset.Value);
            return true;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var inboxStore = scope.ServiceProvider.GetRequiredService<IInboxStore>();
        var writeResult = await inboxStore.StoreAsync(
            new InboxMessageInput(
                envelope.EventId,
                messageType,
                envelope.SchemaVersion,
                result.Message.Key,
                result.Message.Value),
            cancellationToken);

        if (writeResult == InboxWriteResult.Conflict)
        {
            logger.LogError(
                "Обнаружен конфликт payload для Kafka-события {EventId}",
                envelope.EventId);
        }

        return true;
    }

    private Task DelayBeforeRetryAsync(CancellationToken cancellationToken)
    {
        return Task.Delay(options.InitialRetryDelayMilliseconds, cancellationToken);
    }
}
