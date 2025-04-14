namespace CorporateSystem.SharedDocs.Domain.Entities;

public class Document
{
    public int Id { get; init; }
    public int OwnerId { get; init; }
    public required string Title { get; init; }
    public string Content { get; init; } = string.Empty;
    public required DateTimeOffset? ModifiedAt { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}