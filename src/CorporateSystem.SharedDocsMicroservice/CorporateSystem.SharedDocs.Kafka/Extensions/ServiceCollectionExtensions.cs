using Confluent.Kafka;
using CorporateSystem.SharedDocs.Kafka.Implementations;
using CorporateSystem.SharedDocs.Kafka.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CorporateSystem.SharedDocs.Kafka.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaConsumers(this IServiceCollection services)
    {
        return services
            .AddKeyedSingleton<IConsumerHandler<Ignore, string>, UserConsumerHandler>(nameof(UserConsumerHandler));
    }
}