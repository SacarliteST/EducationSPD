using EducationSPD.Infrastructure.Configuration;
using EducationSPD.Infrastructure.Health;
using EducationSPD.Tests.Support;
using Microsoft.Extensions.Options;

namespace EducationSPD.Tests.System;

public sealed class ServiceReadinessProbeTests(PostgresDatabaseFixture database)
    : IClassFixture<PostgresDatabaseFixture>
{
    [Fact]
    public async Task Readiness_RequiresDatabaseMigrationsAndBothKafkaConsumers()
    {
        const string catalogTopic = "catalog";
        const string episodeTopic = "episodes";
        var kafkaState = new KafkaConsumerReadiness();
        var options = Options.Create(new KafkaOptions
        {
            BootstrapServers = "unused",
            ConsumerGroup = "tests",
            SkillCatalogTopic = catalogTopic,
            EpisodeFinalizedTopic = episodeTopic,
            MaxRetryAttempts = 3,
            InitialRetryDelayMilliseconds = 10
        });
        await using var dbContext = database.CreateDbContext();
        var probe = new ServiceReadinessProbe(dbContext, kafkaState, options);

        Assert.False(await probe.IsReadyAsync(CancellationToken.None));

        kafkaState.MarkReady(catalogTopic);
        kafkaState.MarkReady(episodeTopic);
        Assert.True(await probe.IsReadyAsync(CancellationToken.None));

        kafkaState.MarkUnavailable(episodeTopic);
        Assert.False(await probe.IsReadyAsync(CancellationToken.None));
    }
}
