using CorporateSystem.Auth.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace CorporateSystem.Auth.Infrastructure.Extensions;

internal static class DtoValidationExtensions
{
    public static void ShouldBeValid<T>(this AddUserDto dto, ILogger<T> logger)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            logger.LogError($"{nameof(ShouldBeValid)}: email is null or white space");
            throw new ArgumentException("Некорректный email");
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            logger.LogError($"{nameof(ShouldBeValid)}: password is null or white space");
            throw new ArgumentException("Некорректный пароль");
        }
    }
}