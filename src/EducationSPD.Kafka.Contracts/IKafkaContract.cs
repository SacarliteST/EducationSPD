namespace EducationSPD.Kafka.Contracts;

/// <summary>Маркер версионированного Kafka-контракта EducationSPD.</summary>
public interface IKafkaContract
{
    /// <summary>Версия wire-схемы сообщения.</summary>
    int SchemaVersion { get; }
}
