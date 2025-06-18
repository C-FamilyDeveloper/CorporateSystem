namespace CorporateSystem.Auth.Kafka.Interfaces;

public interface IProducerHandler<in TEvent> : IDisposable
{
    Task ProduceAsync(IReadOnlyList<TEvent> data, CancellationToken cancellationToken = default);
}