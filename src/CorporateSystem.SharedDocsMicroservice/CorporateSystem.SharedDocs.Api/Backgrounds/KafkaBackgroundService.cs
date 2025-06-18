using Confluent.Kafka;
using CorporateSystem.SharedDocs.Kafka;
using CorporateSystem.SharedDocs.Kafka.Interfaces;
using CorporateSystem.SharedDocs.Kafka.Options;
using Microsoft.Extensions.Options;

namespace CorporateSystem.SharedDocs.Api.Backgrounds;

public class KafkaBackgroundService : BackgroundService
{
    private readonly ILogger<KafkaBackgroundService> _logger;
    private readonly KafkaAsyncConsumer<Ignore, string> _consumer;

    public KafkaBackgroundService(
        IOptions<KafkaOptions> options,
        ILogger<KafkaBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        var kafkaOptions = options.Value;
        _logger = logger;
        _consumer = new KafkaAsyncConsumer<Ignore, string>(
            kafkaOptions.BootstrapServer,
            kafkaOptions.GroupId,
            kafkaOptions.Topics,
            serviceProvider.GetRequiredKeyedService<IConsumerHandler<Ignore, string>>("UserConsumerHandler"),
            serviceProvider.GetRequiredService<ILogger<KafkaAsyncConsumer<Ignore, string>>>());
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _consumer.ConsumeAsync(stoppingToken);
        }
        catch (Exception e)
        {
            _logger.LogError($"{nameof(KafkaBackgroundService)}: {e.Message}");
        }
    }
}