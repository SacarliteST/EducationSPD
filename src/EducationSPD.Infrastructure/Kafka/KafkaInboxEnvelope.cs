namespace EducationSPD.Infrastructure.Kafka;

internal sealed record KafkaInboxEnvelope(Guid EventId, int SchemaVersion);
