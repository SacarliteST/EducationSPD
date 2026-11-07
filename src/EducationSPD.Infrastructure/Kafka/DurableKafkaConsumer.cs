using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using EducationSPD.Application.Diagnostics;
using EducationSPD.Application.Inbox;
using EducationSPD.Domain.Inbox;
using EducationSPD.Infrastructure.Configuration;
using EducationSPD.Infrastructure.Health;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EducationSPD.Infrastructure.Kafka;

internal abstract class DurableKafkaConsumer(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaOptions> options,
    KafkaConsumerReadiness readiness,
    ILogger logger,
    string topic,
    InboxMessageType messageType) : BackgroundService
{
    private readonly KafkaOptions options = options.Value;
    private int retryAttempt;
    private int consumerError;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = options.BootstrapServers,
            GroupId = options.ConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, _) => Interlocked.Exchange(ref consumerError, 1))
            .Build();
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
                        retryAttempt = 0;
                    }
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    logger.LogError(
                        exception,
                        "Не удалось надёжно сохранить Kafka-сообщение типа {MessageType}",
                        messageType);
                    consumer.Seek(result.TopicPartitionOffset);
                    readiness.MarkUnavailable(topic);
                    await DelayBeforeRetryAsync(stoppingToken);
                }
            }
        }
        finally
        {
            readiness.MarkUnavailable(topic);
            consumer.Close();
            logger.LogInformation("Остановлено чтение Kafka-топика {Topic}", topic);
        }
    }

    private async Task<ConsumeResult<string, string>?> ConsumeAsync(
        IConsumer<string, string> consumer,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Interlocked.Exchange(ref consumerError, 0);
            var result = consumer.Consume(TimeSpan.FromSeconds(1));
            if (Volatile.Read(ref consumerError) != 0)
            {
                readiness.MarkUnavailable(topic);
                await DelayBeforeRetryAsync(cancellationToken);
                return null;
            }

            readiness.MarkReady(topic);
            retryAttempt = 0;
            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return null;
        }
        catch (ConsumeException exception)
        {
            readiness.MarkUnavailable(topic);
            logger.LogWarning(exception, "Не удалось прочитать Kafka-топик {Topic}", topic);
            await DelayBeforeRetryAsync(cancellationToken);
            return null;
        }
    }

    private async Task<bool> StoreAsync(
        ConsumeResult<string, string> result,
        CancellationToken cancellationToken)
    {
        using var activity = StartConsumerActivity(result.Message.Headers);
        if (string.IsNullOrWhiteSpace(result.Message.Value)
            || string.IsNullOrWhiteSpace(result.Message.Key))
        {
            logger.LogError(
                "Отклонено Kafka-сообщение типа {MessageType} без key или payload: offset {Offset}",
                messageType,
                result.Offset.Value);
            EducationSpdTelemetry.RecordKafkaMessage(topic, "invalid");
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
            EducationSpdTelemetry.RecordKafkaMessage(topic, "invalid");
            return true;
        }

        using var logScope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["EventId"] = envelope.EventId
        });

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
            logger.LogError("Обнаружен конфликт payload Kafka-события");
        }

        EducationSpdTelemetry.RecordKafkaMessage(topic, writeResult.ToString().ToLowerInvariant());

        return true;
    }

    private Task DelayBeforeRetryAsync(CancellationToken cancellationToken)
    {
        retryAttempt = Math.Min(retryAttempt + 1, options.MaxRetryAttempts);
        var exponent = retryAttempt - 1;
        var delay = TimeSpan.FromMilliseconds(
            options.InitialRetryDelayMilliseconds * Math.Pow(2, exponent));
        return Task.Delay(delay, cancellationToken);
    }

    private static Activity? StartConsumerActivity(Headers? headers)
    {
        var traceParent = ReadHeader(headers, "traceparent");
        var traceState = ReadHeader(headers, "tracestate");
        return ActivityContext.TryParse(traceParent, traceState, out var parentContext)
            ? EducationSpdTelemetry.ActivitySource.StartActivity(
                "kafka consume",
                ActivityKind.Consumer,
                parentContext)
            : EducationSpdTelemetry.ActivitySource.StartActivity("kafka consume", ActivityKind.Consumer);
    }

    private static string? ReadHeader(Headers? headers, string name)
    {
        var value = headers?.LastOrDefault(header => string.Equals(header.Key, name, StringComparison.OrdinalIgnoreCase))
            ?.GetValueBytes();
        return value is null ? null : Encoding.UTF8.GetString(value);
    }
}
