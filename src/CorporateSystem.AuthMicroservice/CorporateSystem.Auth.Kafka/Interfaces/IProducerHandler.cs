namespace CorporateSystem.Auth.Kafka.Interfaces;

public interface IProducerHandler<in TKey, in TEvent> : IDisposable
{
    Task ProduceAsync(IReadOnlyList<TEvent> data, CancellationToken cancellationToken = default);
}