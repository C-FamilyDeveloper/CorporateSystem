using Confluent.Kafka;
using CorporateSystem.Auth.Kafka.Implementations;
using CorporateSystem.Auth.Kafka.Interfaces;
using CorporateSystem.Auth.Kafka.Models;
using CorporateSystem.Auth.Kafka.Serializers;
using Microsoft.Extensions.DependencyInjection;

namespace CorporateSystem.Auth.Kafka.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKeyedProduceHandler(this IServiceCollection services)
    {
        return services
            .AddSingleton<KafkaAsyncProducer<Null, UserDeleteEvent>>()
            .AddSingleton<ISerializer<UserDeleteEvent>, TextJsonSerializer<UserDeleteEvent>>()
            .AddSingleton<ISerializer<Null>>(_ => Confluent.Kafka.Serializers.Null)
            .AddSingleton<IProducerHandler<Null, UserDeleteEvent>, ProducerHandler<Null, UserDeleteEvent>>();
    }
}