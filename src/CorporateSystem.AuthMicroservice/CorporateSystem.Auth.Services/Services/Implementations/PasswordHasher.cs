using CorporateSystem.Auth.Services.Services.Interfaces;

namespace CorporateSystem.Auth.Services.Services.Implementations;

internal class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}