using CorporateSystem.Auth.Domain.Entities;

namespace CorporateSystem.Auth.Services.Services.Interfaces;

public interface ITokenService
{
    Task<string> GenerateJwtTokenAsync(User user, CancellationToken cancellationToken = default);
    Task<string> GenerateRefreshTokenAsync(GenerateRefreshTokenDto dto, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<RefreshTokenResponse> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);
}

public record struct GenerateRefreshTokenDto(int UserId, string UserIpAddress);
public record struct RefreshTokenRequest(int UserId, string UserIpAddress);
public record struct RefreshTokenResponse(string Token);