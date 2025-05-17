using CorporateSystem.Auth.Domain.Enums;

namespace CorporateSystem.Auth.Services.Services.Interfaces;

public interface IRegistrationService
{
    Task RegisterAsync(RegisterUserDto dto, CancellationToken cancellationToken = default);
    Task SuccessRegisterAsync(SuccessRegisterUserDto dto, CancellationToken cancellationToken = default);
}

public record struct RegisterUserDto(string Email, string Password, string RepeatedPassword);
public record struct SuccessRegisterUserDto(
    string Email, 
    string Password,
    int SuccessCode, 
    string FirstName,
    string LastName,
    Gender Gender);