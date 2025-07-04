using CorporateSystem.Auth.Kafka.Interfaces;

namespace CorporateSystem.Auth.Kafka.Implementations;

internal sealed class EventHandlerFactory(IServiceProvider serviceProvider) : IEventHandlerFactory
{
    public IEventHandler<object> GetHandler(string eventType)
    {
        var handlerType = Type.GetType(eventType);
        if (handlerType is null)
        {
            throw new InvalidOperationException($"Неизвестный тип события: {eventType}");
        }
        
        var handler = serviceProvider.GetService(typeof(IEventHandler<>).MakeGenericType(handlerType));
        if (handler is null)
        {
            throw new InvalidOperationException($"Обработчик для события {eventType} не зарегистрирован");
        }

        return (IEventHandler<object>)handler;
    }
}