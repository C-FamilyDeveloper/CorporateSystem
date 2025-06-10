using CorporateSystem.Auth.Services.Exceptions;
using CorporateSystem.Auth.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CorporateSystem.Auth.Services.Extensions;

internal static class DtoValidationExtensions
{
    public static void ShouldBeValid<T>(this SuccessRegisterUserDto dto, ILogger<T> logger)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            logger.LogError($"{nameof(ShouldBeValid)}: email is null or white space");
            throw new ArgumentException("Некорректный email");
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            logger.LogError($"{nameof(ShouldBeValid)}: password is null or white space");
            throw new AggregateException("Некорректный пароль");
        }
    }

    public static void ShouldBeValid<T>(this RegisterUserDto dto, ILogger<T> logger)
    {
        if (dto.Password != dto.RepeatedPassword)
        {
            logger.LogInformation($"{nameof(ShouldBeValid)}: password={dto.Password}, repeated password={dto.RepeatedPassword}");
            throw new InvalidRegistrationException("Пароли не совпадают");
        }
    }
}