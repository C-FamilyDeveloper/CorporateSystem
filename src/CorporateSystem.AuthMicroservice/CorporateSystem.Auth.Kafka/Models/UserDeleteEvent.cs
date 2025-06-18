using Confluent.Kafka;
using CorporateSystem.Auth.Kafka.Interfaces;

namespace CorporateSystem.Auth.Kafka.Models;

public class UserDeleteEvent : IKeyedEvent<Ignore>
{
    public int UserId { get; init; }
    public Ignore Key { get; init; }
}