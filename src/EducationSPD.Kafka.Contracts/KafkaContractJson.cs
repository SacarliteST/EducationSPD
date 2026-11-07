using System.Text.Json;
using System.Text.Json.Serialization;

namespace EducationSPD.Kafka.Contracts;

/// <summary>Единая JSON-сериализация публичных Kafka-контрактов EducationSPD.</summary>
public static class KafkaContractJson
{
    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    /// <summary>Сериализует и предварительно проверяет Kafka-сообщение.</summary>
    /// <typeparam name="TContract">Поддерживаемый тип Kafka-контракта.</typeparam>
    /// <param name="message">Сообщение для сериализации.</param>
    /// <returns>JSON с camelCase-полями и строковыми enum.</returns>
    public static string Serialize<TContract>(TContract message)
        where TContract : class, IKafkaContract
    {
        KafkaContractValidator.Validate(message);
        return JsonSerializer.Serialize(message, SerializerOptions);
    }

    /// <summary>Сериализует Kafka-сообщение в UTF-8.</summary>
    /// <typeparam name="TContract">Поддерживаемый тип Kafka-контракта.</typeparam>
    /// <param name="message">Сообщение для сериализации.</param>
    /// <returns>JSON-представление сообщения в UTF-8.</returns>
    public static byte[] SerializeToUtf8Bytes<TContract>(TContract message)
        where TContract : class, IKafkaContract
    {
        KafkaContractValidator.Validate(message);
        return JsonSerializer.SerializeToUtf8Bytes(message, SerializerOptions);
    }

    /// <summary>Десериализует и проверяет Kafka-сообщение из JSON.</summary>
    /// <typeparam name="TContract">Ожидаемый тип Kafka-контракта.</typeparam>
    /// <param name="json">JSON-сообщение.</param>
    /// <returns>Проверенный контракт.</returns>
    public static TContract Deserialize<TContract>(string json)
        where TContract : class, IKafkaContract
    {
        var message = JsonSerializer.Deserialize<TContract>(json, SerializerOptions)
            ?? throw new JsonException("Kafka-сообщение не содержит объект.");

        KafkaContractValidator.Validate(message);
        return message;
    }

    /// <summary>Десериализует и проверяет Kafka-сообщение из UTF-8.</summary>
    /// <typeparam name="TContract">Ожидаемый тип Kafka-контракта.</typeparam>
    /// <param name="utf8Json">JSON-сообщение в UTF-8.</param>
    /// <returns>Проверенный контракт.</returns>
    public static TContract Deserialize<TContract>(ReadOnlySpan<byte> utf8Json)
        where TContract : class, IKafkaContract
    {
        var message = JsonSerializer.Deserialize<TContract>(utf8Json, SerializerOptions)
            ?? throw new JsonException("Kafka-сообщение не содержит объект.");

        KafkaContractValidator.Validate(message);
        return message;
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = false,
            NumberHandling = JsonNumberHandling.Strict
        };

        options.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
        return options;
    }
}
