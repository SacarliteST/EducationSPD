using EducationSPD.Domain.Inbox;
using EducationSPD.Infrastructure.Configuration;
using EducationSPD.Infrastructure.Health;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EducationSPD.Infrastructure.Kafka;

internal sealed class SkillCatalogKafkaConsumer(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaOptions> options,
    KafkaConsumerReadiness readiness,
    ILogger<SkillCatalogKafkaConsumer> logger)
    : DurableKafkaConsumer(
        scopeFactory,
        options,
        readiness,
        logger,
        options.Value.SkillCatalogTopic,
        InboxMessageType.SkillCatalogChanged)
{
}
