namespace CorporateSystem.Auth.Services.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> AuthenticateAsync(AuthUserDto dto, CancellationToken cancellationToken = default);
}

public record struct AuthResultDto(string JwtToken);
public record struct AuthUserDto(string Email, string Password, string UserIpAddress);