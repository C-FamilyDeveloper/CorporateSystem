using Confluent.Kafka;
using CorporateSystem.Auth.Kafka.Interfaces;

namespace CorporateSystem.Auth.Kafka.Models;

public class UserDeleteEvent : IKeyedEvent<Null>
{
    public int UserId { get; init; }
    public Null Key { get; init; }
}