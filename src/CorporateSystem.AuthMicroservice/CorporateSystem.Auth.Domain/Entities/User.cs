using CorporateSystem.Auth.Domain.Enums;

namespace CorporateSystem.Auth.Domain.Entities;

public class User
{
    public int Id { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public Gender Gender { get; init; }
    public Role Role { get; init; }
    public List<RefreshToken> RefreshTokens { get; init; }
    public Department Department { get; init; }
    public int DepartmentId { get; init; }
}