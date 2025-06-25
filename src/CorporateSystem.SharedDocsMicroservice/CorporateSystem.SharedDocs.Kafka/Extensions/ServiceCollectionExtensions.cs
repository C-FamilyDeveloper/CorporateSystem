using Confluent.Kafka;
using CorporateSystem.SharedDocs.Kafka.Implementations;
using CorporateSystem.SharedDocs.Kafka.Interfaces;
using CorporateSystem.SharedDocs.Kafka.Models;
using CorporateSystem.SharedDocs.Kafka.Serializers;
using Microsoft.Extensions.DependencyInjection;

namespace CorporateSystem.SharedDocs.Kafka.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaConsumers(this IServiceCollection services)
    {
        return services
            .AddSingleton<KafkaAsyncConsumer<Null, UserDeleteEvent>>()
            .AddSingleton<IDeserializer<UserDeleteEvent>, TextJsonSerializer<UserDeleteEvent>>()
            .AddSingleton<IDeserializer<Null>>(_ => Deserializers.Null)
            .AddKeyedSingleton<IConsumerHandler<Null, UserDeleteEvent>, UserConsumerHandler>(nameof(UserDeleteEvent));
    }
}