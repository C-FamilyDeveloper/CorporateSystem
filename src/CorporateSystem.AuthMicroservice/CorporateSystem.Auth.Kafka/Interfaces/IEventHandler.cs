namespace CorporateSystem.Auth.Kafka.Interfaces;

public interface IEventHandler<in TEvent>
{
    Task HandleAsync(TEvent[] events, CancellationToken cancellationToken = default);
}