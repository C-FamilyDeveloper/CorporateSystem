using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CorporateSystem.Auth.Domain.Entities;
using CorporateSystem.Auth.Infrastructure.Repositories.Interfaces;
using CorporateSystem.Auth.Services.Exceptions;
using CorporateSystem.Auth.Services.Options;
using CorporateSystem.Auth.Services.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CorporateSystem.Auth.Services.Services.Implementations;

internal class JwtTokenService(IContextFactory contextFactory, IOptions<JwtToken> jwtToken) : ITokenService
{
    private readonly JwtToken _jwtToken = jwtToken.Value;
    
    public Task<string> GenerateJwtTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(GenerateJwtToken(user));
    }

    public Task<string> GenerateRefreshTokenAsync(GenerateRefreshTokenDto dto, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        var refreshToken = Convert.ToBase64String(randomNumber);

        refreshToken = $"{dto.UserId}:{dto.UserIpAddress}:{refreshToken}";

        return Task.FromResult(refreshToken);
    }
    
    public Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtToken.JwtSecret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = true,
                RoleClaimType = ClaimTypes.Role
            }, out var validatedToken);

            return Task.FromResult(validatedToken != null);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<RefreshTokenResponse> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        return contextFactory.ExecuteWithoutCommitAsync(async context =>
        {
            var currentRefreshToken = await context.RefreshTokens
                .Include(token => token.User)
                .FirstOrDefaultAsync(token =>
                    token.UserId == request.UserId && token.IpAddress == request.UserIpAddress,
                    cancellationToken);

            var now = DateTimeOffset.UtcNow;

            if (currentRefreshToken == null || currentRefreshToken.ExpiryOn < now)
            {
                throw new RefreshTokenExpiredException();
            }

            var token = GenerateJwtToken(currentRefreshToken.User);

            return new RefreshTokenResponse(token);
        }, cancellationToken: cancellationToken);
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtToken.JwtSecret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            ]),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}