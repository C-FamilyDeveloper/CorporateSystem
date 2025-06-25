using Confluent.Kafka;

namespace CorporateSystem.SharedDocs.Kafka.Models;

public class UserDeleteEvent : IKeyedEvent<Null>
{
    public int UserId { get; init; }
    public Null Key { get; init; }
}