using EducationSPD.Domain.Inbox;

namespace EducationSPD.Application.Inbox;

/// <summary>Транспортное сообщение для сохранения в durable inbox.</summary>
/// <param name="EventId">Идентификатор интеграционного события.</param>
/// <param name="MessageType">Тип сообщения.</param>
/// <param name="SchemaVersion">Версия wire-схемы.</param>
/// <param name="MessageKey">Ключ Kafka-сообщения.</param>
/// <param name="Payload">Исходный JSON payload.</param>
public sealed record InboxMessageInput(
    Guid EventId,
    InboxMessageType MessageType,
    int SchemaVersion,
    string MessageKey,
    string Payload);
