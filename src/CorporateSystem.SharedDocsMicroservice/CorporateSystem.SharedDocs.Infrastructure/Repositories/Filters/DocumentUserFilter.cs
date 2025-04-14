using CorporateSystem.SharedDocs.Domain.Enums;

namespace CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;

public record DocumentUserFilter
{
    public int[]? Ids { get; set; } = null;
    public int[]? DocumentIds { get; set; } = null;
    public int[]? UserIds { get; set; } = null;
    public AccessLevel[]? AccessLevels { get; set; } = null;
}