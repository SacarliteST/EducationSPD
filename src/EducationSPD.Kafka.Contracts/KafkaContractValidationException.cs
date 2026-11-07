namespace EducationSPD.Kafka.Contracts;

/// <summary>Ошибка семантической проверки Kafka-сообщения.</summary>
public sealed class KafkaContractValidationException : Exception
{
    /// <summary>Создаёт ошибку с безопасным описанием нарушенного контракта.</summary>
    /// <param name="message">Описание нарушения wire-контракта.</param>
    public KafkaContractValidationException(string message)
        : base(message)
    {
    }
}
