using System.Text.Json;
using Confluent.Kafka;

namespace CorporateSystem.Auth.Kafka.Serializers;

internal class TextJsonSerializer<T> : ISerializer<T>
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data);
    }
}