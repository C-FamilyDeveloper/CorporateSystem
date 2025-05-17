namespace CorporateSystem.SharedDocs.Domain.Entities;

public class DocumentChangeLog
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public int DocumentId { get; init; }
    public DateTimeOffset ChangedAt { get; init; }
    public required string Changes { get; init; }
    public int Line { get; init; }
}