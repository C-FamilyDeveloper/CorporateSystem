using System.Runtime.CompilerServices;
using CorporateSystem.Auth.Kafka.Interfaces;

[assembly: InternalsVisibleTo("CorporateSystem.Auth.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace CorporateSystem.Auth.Kafka;

public interface IKafkaAsyncProducer<TKey, in TEvent> : IDisposable
    where TEvent : IKeyedEvent<TKey>
{
    Task ProduceAsync(IReadOnlyList<TEvent> data, CancellationToken cancellationToken = default);
}

internal class KafkaAsyncProducer<TKey, TEvent>(
    IProducerHandler<TKey, TEvent> producerHandler) : IKafkaAsyncProducer<TKey, TEvent>
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