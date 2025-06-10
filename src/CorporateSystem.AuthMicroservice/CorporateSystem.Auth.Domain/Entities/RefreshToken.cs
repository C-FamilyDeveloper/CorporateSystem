namespace CorporateSystem.Auth.Domain.Entities;

public class RefreshToken
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public required string Token { get; set; }
    public required string IpAddress { get; init; }
    public required User User { get; init; }
    public DateTimeOffset ExpiryOn { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
}