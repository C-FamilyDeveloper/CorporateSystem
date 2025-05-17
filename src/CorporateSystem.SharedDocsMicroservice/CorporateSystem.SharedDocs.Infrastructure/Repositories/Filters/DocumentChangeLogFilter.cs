namespace CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;

public class DocumentChangeLogFilter
{
    public int[]? Ids { get; set; } = null;
    public int[]? UserIds { get; set; } = null;
    public int[]? DocumentIds { get; set; } = null;
}