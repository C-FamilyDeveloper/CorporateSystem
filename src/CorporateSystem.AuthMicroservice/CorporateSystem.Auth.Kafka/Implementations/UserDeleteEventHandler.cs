using Confluent.Kafka;
using CorporateSystem.Auth.Kafka.Interfaces;
using CorporateSystem.Auth.Kafka.Models;

namespace CorporateSystem.Auth.Kafka.Implementations;

internal sealed class UserDeleteEventHandler(IKafkaAsyncProducer<Null, UserDeleteEvent> kafkaAsyncProducer) 
    : IEventHandler<UserDeleteEvent>
{
    public Task HandleAsync(UserDeleteEvent[] events, CancellationToken cancellationToken = default)
    {
        return kafkaAsyncProducer.ProduceAsync(events, cancellationToken);
    }
}