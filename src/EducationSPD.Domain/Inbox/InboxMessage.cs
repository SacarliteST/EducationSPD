namespace EducationSPD.Domain.Inbox;

/// <summary>Надёжно сохранённое интеграционное сообщение.</summary>
public sealed class InboxMessage
{
    /// <summary>Максимальная длина Kafka message key.</summary>
    public const int MessageKeyMaxLength = 250;

    /// <summary>Длина SHA-256 в шестнадцатеричном представлении.</summary>
    public const int PayloadHashLength = 64;

    /// <summary>Максимальная длина безопасного кода ошибки.</summary>
    public const int ErrorCodeMaxLength = 100;

    private InboxMessage()
    {
    }

    private InboxMessage(
        Guid eventId,
        InboxMessageType messageType,
        int schemaVersion,
        string messageKey,
        string payload,
        string payloadHash,
        DateTimeOffset receivedAt)
    {
        EventId = eventId;
        MessageType = messageType;
        SchemaVersion = schemaVersion;
        MessageKey = messageKey;
        Payload = payload;
        PayloadHash = payloadHash;
        Status = InboxStatus.Received;
        ReceivedAt = receivedAt;
    }

    /// <summary>Идентификатор интеграционного события и доставки.</summary>
    public Guid EventId { get; private set; }

    /// <summary>Тип интеграционного сообщения.</summary>
    public InboxMessageType MessageType { get; private set; }

    /// <summary>Версия wire-схемы сообщения.</summary>
    public int SchemaVersion { get; private set; }

    /// <summary>Ключ Kafka-сообщения.</summary>
    public string MessageKey { get; private set; } = string.Empty;

    /// <summary>Исходный JSON payload.</summary>
    public string Payload { get; private set; } = string.Empty;

    /// <summary>SHA-256 исходного payload.</summary>
    public string PayloadHash { get; private set; } = string.Empty;

    /// <summary>Текущее состояние обработки.</summary>
    public InboxStatus Status { get; private set; }

    /// <summary>Безопасный код окончательной ошибки.</summary>
    public string? ErrorCode { get; private set; }

    /// <summary>Момент сохранения сообщения в UTC.</summary>
    public DateTimeOffset ReceivedAt { get; private set; }

    /// <summary>Момент окончательного применения или отклонения в UTC.</summary>
    public DateTimeOffset? ProcessedAt { get; private set; }

    /// <summary>Количество технических повторов обработки.</summary>
    public int RetryCount { get; private set; }

    /// <summary>Необязательный момент следующей попытки в UTC.</summary>
    public DateTimeOffset? NextRetryAt { get; private set; }

    /// <summary>Создаёт полученное сообщение в начальном состоянии.</summary>
    public static InboxMessage Receive(
        Guid eventId,
        InboxMessageType messageType,
        int schemaVersion,
        string messageKey,
        string payload,
        string payloadHash,
        DateTimeOffset receivedAt)
    {
        RequireGuid(eventId);
        RequireDefined(messageType, nameof(messageType));

        if (schemaVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(schemaVersion), "Версия схемы должна быть положительной.");
        }

        RequireText(messageKey, nameof(messageKey), MessageKeyMaxLength);
        RequireText(payload, nameof(payload), int.MaxValue);
        RequirePayloadHash(payloadHash);
        RequireUtc(receivedAt, nameof(receivedAt));

        return new InboxMessage(
            eventId,
            messageType,
            schemaVersion,
            messageKey,
            payload,
            payloadHash,
            receivedAt);
    }

    /// <summary>Переводит сообщение в ожидание каталога навыков.</summary>
    public void WaitForCatalog(DateTimeOffset nextRetryAt)
    {
        RequireProcessable();
        RequireUtc(nextRetryAt, nameof(nextRetryAt));

        if (nextRetryAt <= ReceivedAt)
        {
            throw new ArgumentOutOfRangeException(nameof(nextRetryAt), "Следующая попытка должна быть позже получения сообщения.");
        }

        Status = InboxStatus.PendingCatalog;
        ErrorCode = null;
        ProcessedAt = null;
        NextRetryAt = nextRetryAt;
    }

    /// <summary>Планирует повтор после временной технической ошибки.</summary>
    public void ScheduleRetry(DateTimeOffset nextRetryAt)
    {
        RequireProcessable();
        RequireUtc(nextRetryAt, nameof(nextRetryAt));

        if (nextRetryAt <= ReceivedAt)
        {
            throw new ArgumentOutOfRangeException(nameof(nextRetryAt), "Следующая попытка должна быть позже получения сообщения.");
        }

        RetryCount++;
        NextRetryAt = nextRetryAt;
    }

    /// <summary>Отмечает сообщение успешно применённым.</summary>
    public void MarkApplied(DateTimeOffset processedAt)
    {
        Complete(InboxStatus.Applied, processedAt, null);
    }

    /// <summary>Отмечает сообщение окончательно отклонённым.</summary>
    public void MarkFailed(string errorCode, DateTimeOffset processedAt)
    {
        RequireText(errorCode, nameof(errorCode), ErrorCodeMaxLength);
        Complete(InboxStatus.Failed, processedAt, errorCode);
    }

    /// <summary>Возвращает ошибочное сообщение в очередь по операционной команде.</summary>
    public void Restart(DateTimeOffset restartedAt)
    {
        if (Status != InboxStatus.Failed)
        {
            throw new InvalidOperationException("Повторно запустить можно только окончательно отклонённое сообщение.");
        }

        RequireUtc(restartedAt, nameof(restartedAt));
        Status = InboxStatus.Received;
        ErrorCode = null;
        ProcessedAt = null;
        RetryCount = 0;
        NextRetryAt = restartedAt;
    }

    private void Complete(InboxStatus status, DateTimeOffset processedAt, string? errorCode)
    {
        RequireProcessable();
        RequireUtc(processedAt, nameof(processedAt));

        if (processedAt < ReceivedAt)
        {
            throw new ArgumentOutOfRangeException(nameof(processedAt), "Время обработки не может быть раньше получения сообщения.");
        }

        Status = status;
        ErrorCode = errorCode;
        ProcessedAt = processedAt;
        NextRetryAt = null;
    }

    private void RequireProcessable()
    {
        if (Status is not InboxStatus.Received and not InboxStatus.PendingCatalog)
        {
            throw new InvalidOperationException($"Сообщение в состоянии {Status} больше нельзя обрабатывать.");
        }
    }

    private static void RequireGuid(Guid eventId)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("EventId не может быть пустым UUID.", nameof(eventId));
        }
    }

    private static void RequirePayloadHash(string payloadHash)
    {
        if (payloadHash.Length != PayloadHashLength
            || payloadHash.Any(character => !Uri.IsHexDigit(character)))
        {
            throw new ArgumentException("PayloadHash должен содержать SHA-256 в hex-формате.", nameof(payloadHash));
        }
    }

    private static void RequireText(string value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maxLength)
        {
            throw new ArgumentException($"Значение должно содержать от 1 до {maxLength} символов.", parameterName);
        }
    }

    private static void RequireUtc(DateTimeOffset value, string parameterName)
    {
        if (value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Время должно быть указано в UTC.", parameterName);
        }
    }

    private static void RequireDefined<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, "Значение enum не поддерживается.");
        }
    }
}
