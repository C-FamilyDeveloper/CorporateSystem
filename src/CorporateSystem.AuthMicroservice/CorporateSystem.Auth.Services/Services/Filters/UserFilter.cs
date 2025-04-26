using CorporateSystem.Auth.Domain.Enums;

namespace CorporateSystem.Auth.Services.Services.Filters;

public class UserFilter
{
    public int[]? Ids { get; init; }
    public string[]? Emails { get; init; }
    public string[]? Passwords { get; init; }
    public Role[]? Roles { get; init; }
}