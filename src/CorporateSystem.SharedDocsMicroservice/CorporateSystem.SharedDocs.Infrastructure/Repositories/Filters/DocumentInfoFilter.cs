namespace CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;

public class DocumentInfoFilter
{
    public int[]? OwnerIds { get; init; }
    public int[]? FollowerIds { get; init; }
}