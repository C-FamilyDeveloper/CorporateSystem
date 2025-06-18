using System.Text.Json.Serialization;

namespace CorporateSystem.SharedDocs.Kafka.Models;

public interface IKeyedEvent<TKey>
{
    [JsonIgnore]
    public TKey Key { get; init; }
}