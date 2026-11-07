using System.Text.Json;

namespace EducationSPD.Infrastructure.Kafka;

internal static class KafkaInboxEnvelopeReader
{
    public static KafkaInboxEnvelope Read(string payload)
    {
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("Kafka payload должен содержать JSON-объект.");
        }

        if (!root.TryGetProperty("eventId", out var eventIdElement)
            || !eventIdElement.TryGetGuid(out var eventId)
            || eventId == Guid.Empty)
        {
            throw new JsonException("Kafka payload не содержит корректный eventId.");
        }

        if (!root.TryGetProperty("schemaVersion", out var schemaVersionElement)
            || !schemaVersionElement.TryGetInt32(out var schemaVersion)
            || schemaVersion <= 0)
        {
            throw new JsonException("Kafka payload не содержит корректный schemaVersion.");
        }

        return new KafkaInboxEnvelope(eventId, schemaVersion);
    }
}
