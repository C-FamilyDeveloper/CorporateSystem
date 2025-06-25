using Confluent.Kafka;

namespace CorporateSystem.SharedDocs.Kafka.Interfaces;

public interface IConsumerHandler<TKey, TValue>
{
    Task Handle(IReadOnlyList<ConsumeResult<TKey, TValue>> messages, CancellationToken cancellationToken = default);
}