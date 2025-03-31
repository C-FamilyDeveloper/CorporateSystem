using CorporateSystem.Auth.Domain.Enums;

namespace CorporateSystem.Auth.Domain.Entities;

public class User
{
    public int Id { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public Role Role { get; init; }
}