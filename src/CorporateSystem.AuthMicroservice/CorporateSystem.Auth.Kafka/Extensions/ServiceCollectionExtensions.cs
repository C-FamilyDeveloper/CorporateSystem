using Confluent.Kafka;
using CorporateSystem.Auth.Kafka.Implementations;
using CorporateSystem.Auth.Kafka.Interfaces;
using CorporateSystem.Auth.Kafka.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CorporateSystem.Auth.Kafka.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKeyedProduceHandler(this IServiceCollection services)
    {
        return services
            .AddKeyedSingleton<IProducerHandler<UserDeleteEvent>, ProducerHandler<Ignore, UserDeleteEvent>>(
                nameof(UserDeleteEvent));
    }
}