using EducationSPD.Application.System;
using EducationSPD.Infrastructure.Configuration;
using EducationSPD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EducationSPD.Infrastructure.Health;

internal sealed class ServiceReadinessProbe(
    AppDbContext dbContext,
    KafkaConsumerReadiness kafkaReadiness,
    IOptions<KafkaOptions> kafkaOptions) : IServiceReadinessProbe
{
    public async Task<bool> IsReadyAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!await dbContext.Database.CanConnectAsync(cancellationToken)
                || (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            {
                return false;
            }

            var options = kafkaOptions.Value;
            return kafkaReadiness.Includes(options.SkillCatalogTopic, options.EpisodeFinalizedTopic);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return false;
        }
    }
}
