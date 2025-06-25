using CorporateSystem.Auth.Kafka.Interfaces;

namespace CorporateSystem.Auth.Kafka;

public sealed class KafkaAsyncProducer<TKey, TEvent>(
    IProducerHandler<TKey, TEvent> producerHandler) : IDisposable
    where TEvent : IKeyedEvent<TKey>
{
    public Task ProduceAsync(IReadOnlyList<TEvent> data, CancellationToken cancellationToken = default)
    {
        return producerHandler.ProduceAsync(data, cancellationToken);
    }

    public void Dispose()
    {
        producerHandler.Dispose();
    }
}