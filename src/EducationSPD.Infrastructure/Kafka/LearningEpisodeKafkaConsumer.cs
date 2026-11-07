using EducationSPD.Domain.Inbox;
using EducationSPD.Infrastructure.Configuration;
using EducationSPD.Infrastructure.Health;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EducationSPD.Infrastructure.Kafka;

internal sealed class LearningEpisodeKafkaConsumer(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaOptions> options,
    KafkaConsumerReadiness readiness,
    ILogger<LearningEpisodeKafkaConsumer> logger)
    : DurableKafkaConsumer(
        scopeFactory,
        options,
        readiness,
        logger,
        options.Value.EpisodeFinalizedTopic,
        InboxMessageType.LearningEpisodeFinalized)
{
}
