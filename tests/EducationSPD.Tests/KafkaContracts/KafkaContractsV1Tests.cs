using System.Text.Json;
using EducationSPD.Kafka.Contracts;
using EducationSPD.Kafka.Contracts.V1;

namespace EducationSPD.Tests.KafkaContracts;

public sealed class KafkaContractsV1Tests
{
    private const string SkillCatalogJson = """
        {
          "schemaVersion": 1,
          "eventId": "00000000-0000-0000-0000-000000000001",
          "moduleKey": "sql",
          "skillId": "join",
          "name": "Использование JOIN",
          "description": "Соединение данных нескольких таблиц",
          "origin": "System",
          "isDeleted": false,
          "occurredAt": "2026-11-06T12:00:00Z"
        }
        """;

    private const string LearningEpisodeJson = """
        {
          "schemaVersion": 1,
          "eventId": "00000000-0000-0000-0000-000000000002",
          "episodeId": "00000000-0000-0000-0000-000000000003",
          "studentId": "00000000-0000-0000-0000-000000000004",
          "moduleKey": "sql",
          "taskId": "00000000-0000-0000-0000-000000000005",
          "finalizedAt": "2026-11-06T12:10:00Z",
          "skillResults": [
            {
              "skillId": "join",
              "source": "ValidationRule",
              "outcome": "Succeeded"
            }
          ]
        }
        """;

    [Fact]
    public void SkillCatalogChanged_ExamplePreservesWireContract()
    {
        var message = KafkaContractJson.Deserialize<SkillCatalogChanged>(SkillCatalogJson);

        Assert.Equal(KafkaSchemaVersions.Version1, message.SchemaVersion);
        Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000001"), message.EventId);
        Assert.Equal("sql", message.ModuleKey);
        Assert.Equal("join", message.SkillId);
        Assert.Equal("Использование JOIN", message.Name);
        Assert.Equal("Соединение данных нескольких таблиц", message.Description);
        Assert.Equal(SkillOrigin.System, message.Origin);
        Assert.False(message.IsDeleted);
        Assert.Equal(DateTimeOffset.Parse("2026-11-06T12:00:00Z"), message.OccurredAt);

        using var serialized = JsonDocument.Parse(KafkaContractJson.Serialize(message));
        var root = serialized.RootElement;
        Assert.Equal("System", root.GetProperty("origin").GetString());
        Assert.Equal("sql", root.GetProperty("moduleKey").GetString());
        Assert.False(root.TryGetProperty("Origin", out _));
    }

    [Fact]
    public void LearningEpisodeFinalized_ExamplePreservesWireContract()
    {
        var message = KafkaContractJson.Deserialize<LearningEpisodeFinalized>(LearningEpisodeJson);

        Assert.Equal(KafkaSchemaVersions.Version1, message.SchemaVersion);
        Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000003"), message.EpisodeId);
        var result = Assert.Single(message.SkillResults);
        Assert.Equal("join", result.SkillId);
        Assert.Equal(SkillResultSource.ValidationRule, result.Source);
        Assert.Equal(SkillOutcome.Succeeded, result.Outcome);

        var utf8Json = KafkaContractJson.SerializeToUtf8Bytes(message);
        var roundTrip = KafkaContractJson.Deserialize<LearningEpisodeFinalized>(utf8Json);
        Assert.Equal(message.EventId, roundTrip.EventId);
        Assert.Equal(message.EpisodeId, roundTrip.EpisodeId);
        Assert.Equal(message.FinalizedAt, roundTrip.FinalizedAt);
        Assert.Equal(message.SkillResults, roundTrip.SkillResults);
    }

    [Fact]
    public void SkillCatalogChanged_NullDescriptionRemainsExplicitlyNullable()
    {
        var message = new SkillCatalogChanged(
            KafkaSchemaVersions.Version1,
            Guid.NewGuid(),
            "sql",
            "join",
            "JOIN",
            null,
            SkillOrigin.TeacherDefined,
            false,
            DateTimeOffset.UtcNow);

        using var serialized = JsonDocument.Parse(KafkaContractJson.Serialize(message));

        Assert.Equal(JsonValueKind.Null, serialized.RootElement.GetProperty("description").ValueKind);
    }

    [Fact]
    public void Deserialize_UnknownSchemaVersionIsRejected()
    {
        var json = SkillCatalogJson.Replace("\"schemaVersion\": 1", "\"schemaVersion\": 2");

        var exception = Assert.Throws<KafkaContractValidationException>(
            () => KafkaContractJson.Deserialize<SkillCatalogChanged>(json));

        Assert.Contains("не поддерживается", exception.Message);
    }

    [Fact]
    public void Deserialize_UnknownStringEnumIsRejected()
    {
        var json = LearningEpisodeJson.Replace("\"outcome\": \"Succeeded\"", "\"outcome\": \"PartiallySucceeded\"");

        Assert.Throws<JsonException>(() => KafkaContractJson.Deserialize<LearningEpisodeFinalized>(json));
    }

    [Fact]
    public void Deserialize_NumericEnumIsRejected()
    {
        var json = SkillCatalogJson.Replace("\"origin\": \"System\"", "\"origin\": 1");

        Assert.Throws<JsonException>(() => KafkaContractJson.Deserialize<SkillCatalogChanged>(json));
    }

    [Fact]
    public void Deserialize_MissingRequiredBooleanIsRejected()
    {
        var json = SkillCatalogJson.Replace("\"isDeleted\": false", "\"anotherField\": false");

        Assert.Throws<JsonException>(() => KafkaContractJson.Deserialize<SkillCatalogChanged>(json));
    }

    [Fact]
    public void Serialize_DuplicateSkillIdIsRejected()
    {
        var message = new LearningEpisodeFinalized(
            KafkaSchemaVersions.Version1,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "sql",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            [
                new SkillResult("join", SkillResultSource.ValidationRule, SkillOutcome.Succeeded),
                new SkillResult("join", SkillResultSource.TaskTag, SkillOutcome.Failed)
            ]);

        Assert.Throws<KafkaContractValidationException>(() => KafkaContractJson.Serialize(message));
    }
}
