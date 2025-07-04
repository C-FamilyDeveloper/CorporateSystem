namespace CorporateSystem.Auth.Kafka.Interfaces;

public interface IEventHandlerFactory
{
    IEventHandler<object> GetHandler(string eventType);
}