using EducationSPD.Domain.Inbox;
using EducationSPD.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EducationSPD.Infrastructure.Kafka;

internal sealed class SkillCatalogKafkaConsumer(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaOptions> options,
    ILogger<SkillCatalogKafkaConsumer> logger)
    : DurableKafkaConsumer(
        scopeFactory,
        options,
        logger,
        options.Value.SkillCatalogTopic,
        InboxMessageType.SkillCatalogChanged)
{
}
