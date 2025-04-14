namespace CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;

public record DocumentFilter
{
    public int[]? Ids { get; set; } = null;
    public int[]? OwnerIds { get; set; } = null;
    public string[]? Titles { get; set; } = null;
    public string[]? Contents { get; set; } = null;
    public DateTimeOffset[]? ModifiedAt { get; set; } = null;
    public DateTimeOffset[]? CreatedAt { get; set; } = null;
}