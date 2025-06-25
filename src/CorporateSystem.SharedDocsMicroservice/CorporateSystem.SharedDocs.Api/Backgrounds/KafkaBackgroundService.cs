using Confluent.Kafka;
using CorporateSystem.SharedDocs.Kafka;
using CorporateSystem.SharedDocs.Kafka.Models;

namespace CorporateSystem.SharedDocs.Api.Backgrounds;

public class KafkaBackgroundService(
    ILogger<KafkaBackgroundService> logger,
    KafkaAsyncConsumer<Null, UserDeleteEvent> consumer)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation($"{nameof(KafkaBackgroundService)}: I woke up!");
            await consumer.ConsumeAsync(stoppingToken);
        }
        catch (Exception e)
        {
            logger.LogError($"{nameof(KafkaBackgroundService)}: {e.Message}");
        }
    }
}