using Confluent.Kafka;

namespace CorporateSystem.SharedDocs.Kafka.Models;

public class UserDeleteEvent : IKeyedEvent<Ignore>
{
    public int UserId { get; init; }
    public Ignore Key { get; init; }
}