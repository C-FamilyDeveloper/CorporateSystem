using System.Text.Json.Serialization;

namespace CorporateSystem.Auth.Kafka.Interfaces;

public interface IKeyedEvent;

public interface IKeyedEvent<TKey> : IKeyedEvent
{
    [JsonIgnore]
    public TKey Key { get; init; }
}