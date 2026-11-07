namespace EducationSPD.Infrastructure.Health;

internal sealed class KafkaConsumerReadiness
{
    private readonly HashSet<string> readyTopics = new(StringComparer.Ordinal);
    private readonly Lock syncRoot = new();

    public void MarkReady(string topic)
    {
        lock (syncRoot)
        {
            readyTopics.Add(topic);
        }
    }

    public void MarkUnavailable(string topic)
    {
        lock (syncRoot)
        {
            readyTopics.Remove(topic);
        }
    }

    public bool Includes(string firstTopic, string secondTopic)
    {
        lock (syncRoot)
        {
            return readyTopics.Contains(firstTopic) && readyTopics.Contains(secondTopic);
        }
    }
}
