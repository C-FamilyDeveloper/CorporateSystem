namespace CorporateSystem.Notification.Domain.Entities;

public class EmailMessage
{
    public int Id { get; init; }
    public required string ReceiverEmail { get; init; }
    public required string Message { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required string SenderEmail { get; init; }
}