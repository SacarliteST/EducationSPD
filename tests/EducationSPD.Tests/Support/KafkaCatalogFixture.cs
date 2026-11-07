using Confluent.Kafka;
using EducationSPD.Application;
using EducationSPD.Infrastructure;
using EducationSPD.Infrastructure.Kafka;
using EducationSPD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.Kafka;
using Testcontainers.PostgreSql;

namespace EducationSPD.Tests.Support;

public sealed class KafkaCatalogFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
    private readonly KafkaContainer kafka = new KafkaBuilder("confluentinc/cp-kafka:7.3.0").Build();
    private ServiceProvider? serviceProvider;
    private SkillCatalogKafkaConsumer? consumer;
    private SkillCatalogInboxProcessor? processor;
    private LearningEpisodeKafkaConsumer? episodeConsumer;
    private LearningEpisodeInboxProcessor? episodeProcessor;
    private bool consumerStarted;
    private bool processorStarted;
    private bool episodeConsumerStarted;
    private bool episodeProcessorStarted;

    public string SkillCatalogTopic { get; } = $"learning.skill-catalog.{Guid.NewGuid():N}";

    public string EpisodeFinalizedTopic { get; } = $"learning.episode-finalized.{Guid.NewGuid():N}";

    public async Task InitializeAsync()
    {
        await Task.WhenAll(postgres.StartAsync(), kafka.StartAsync());

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = postgres.GetConnectionString(),
                ["Kafka:BootstrapServers"] = kafka.GetBootstrapAddress(),
                ["Kafka:ConsumerGroup"] = $"education-spd-tests-{Guid.NewGuid():N}",
                ["Kafka:SkillCatalogTopic"] = SkillCatalogTopic,
                ["Kafka:EpisodeFinalizedTopic"] = EpisodeFinalizedTopic,
                ["Kafka:MaxRetryAttempts"] = "10",
                ["Kafka:InitialRetryDelayMilliseconds"] = "100",
                ["Inbox:BatchSize"] = "10",
                ["Inbox:PendingCatalogRetryIntervalSeconds"] = "1",
                ["Model:InitialMasteryProbability"] = "0.2",
                ["Model:LearnProbability"] = "0.1",
                ["Model:ValidationRuleGuessProbability"] = "0.1",
                ["Model:ValidationRuleSlipProbability"] = "0.1",
                ["Model:TaskTagGuessProbability"] = "0.25",
                ["Model:TaskTagSlipProbability"] = "0.25",
                ["Model:AhpMatrix:0:0"] = "1"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddApplication();
        services.AddInfrastructure(configuration);
        serviceProvider = services.BuildServiceProvider();

        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        var hostedServices = serviceProvider.GetServices<IHostedService>().ToArray();
        consumer = Assert.Single(hostedServices.OfType<SkillCatalogKafkaConsumer>());
        processor = Assert.Single(hostedServices.OfType<SkillCatalogInboxProcessor>());
        episodeConsumer = Assert.Single(hostedServices.OfType<LearningEpisodeKafkaConsumer>());
        episodeProcessor = Assert.Single(hostedServices.OfType<LearningEpisodeInboxProcessor>());
    }

    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .Options;
        return new AppDbContext(options);
    }

    public async Task StartConsumerAsync()
    {
        if (consumerStarted)
        {
            return;
        }

        await consumer!.StartAsync(CancellationToken.None);
        consumerStarted = true;
    }

    public async Task StartProcessorAsync()
    {
        if (processorStarted)
        {
            return;
        }

        await processor!.StartAsync(CancellationToken.None);
        processorStarted = true;
    }

    public async Task StartEpisodeConsumerAsync()
    {
        if (episodeConsumerStarted)
        {
            return;
        }

        await episodeConsumer!.StartAsync(CancellationToken.None);
        episodeConsumerStarted = true;
    }

    public async Task StartEpisodeProcessorAsync()
    {
        if (episodeProcessorStarted)
        {
            return;
        }

        await episodeProcessor!.StartAsync(CancellationToken.None);
        episodeProcessorStarted = true;
    }

    public async Task PublishAsync(string key, string payload)
    {
        await PublishAsync(SkillCatalogTopic, key, payload);
    }

    public async Task PublishAsync(string topic, string key, string payload)
    {
        using var producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = kafka.GetBootstrapAddress()
        }).Build();

        await producer.ProduceAsync(
            topic,
            new Message<string, string> { Key = key, Value = payload });
    }

    public async Task DisposeAsync()
    {
        if (episodeProcessorStarted)
        {
            await episodeProcessor!.StopAsync(CancellationToken.None);
        }

        if (episodeConsumerStarted)
        {
            await episodeConsumer!.StopAsync(CancellationToken.None);
        }

        if (processorStarted)
        {
            await processor!.StopAsync(CancellationToken.None);
        }

        if (consumerStarted)
        {
            await consumer!.StopAsync(CancellationToken.None);
        }

        if (serviceProvider is not null)
        {
            await serviceProvider.DisposeAsync();
        }

        await kafka.DisposeAsync();
        await postgres.DisposeAsync();
    }
}
