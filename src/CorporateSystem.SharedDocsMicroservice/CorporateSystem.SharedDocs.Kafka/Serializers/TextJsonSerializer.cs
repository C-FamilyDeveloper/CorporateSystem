using System.Text.Json;
using Confluent.Kafka;

namespace CorporateSystem.SharedDocs.Kafka.Serializers;

internal class TextJsonSerializer<T> : IDeserializer<T>
{
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return isNull
            ? throw new ArgumentNullException($"Null data encountered deserializing {typeof(T).Name} value.")
            : JsonSerializer.Deserialize<T>(data)!;
    }
}