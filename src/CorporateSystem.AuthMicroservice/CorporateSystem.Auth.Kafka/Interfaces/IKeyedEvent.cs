using System.Text.Json.Serialization;

namespace CorporateSystem.Auth.Kafka.Interfaces;

public interface IKeyedEvent<TKey>
{
    [JsonIgnore]
    public TKey Key { get; init; }
}