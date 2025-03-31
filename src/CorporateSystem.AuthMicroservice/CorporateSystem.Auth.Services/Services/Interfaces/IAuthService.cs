namespace CorporateSystem.Auth.Services.Services.Interfaces;

public interface IAuthService
{
    Task<string> AuthenticateAsync(AuthUserDto dto, CancellationToken cancellationToken = default);
    bool ValidateToken(string token);
}

public record struct AuthUserDto(string Email, string Password);