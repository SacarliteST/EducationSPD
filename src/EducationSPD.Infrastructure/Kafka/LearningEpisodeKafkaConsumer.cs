using EducationSPD.Domain.Inbox;
using EducationSPD.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EducationSPD.Infrastructure.Kafka;

internal sealed class LearningEpisodeKafkaConsumer(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaOptions> options,
    ILogger<LearningEpisodeKafkaConsumer> logger)
    : DurableKafkaConsumer(
        scopeFactory,
        options,
        logger,
        options.Value.EpisodeFinalizedTopic,
        InboxMessageType.LearningEpisodeFinalized)
{
}
