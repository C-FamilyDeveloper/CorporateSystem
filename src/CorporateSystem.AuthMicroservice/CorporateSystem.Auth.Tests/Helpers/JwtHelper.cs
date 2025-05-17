using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CorporateSystem.Auth.Domain.Enums;
using Microsoft.IdentityModel.Tokens;

namespace CorporateSystem.Auth.Tests.Helpers;

public static class JwtHelper
{
    public static string GenerateJwtToken(
        string secretKey,
        int id,
        string email,
        Role role = Role.User,
        DateTime? expires = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role.ToString())
            ]),
            Expires = expires ?? DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}