namespace CorporateSystem.Auth.Domain.Entities;

public class OutboxEvent
{
    public int Id { get; set; }
    public required string EventType { get; set; }
    public required string Payload { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool Processed { get; set; } = false;
}