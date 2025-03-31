using CorporateSystem.Auth.Domain.Entities;
using CorporateSystem.Auth.Domain.Enums;

namespace CorporateSystem.Auth.Infrastructure.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddUserAsync(AddUserDto dto, CancellationToken cancellationToken = default);
}

public record struct AddUserDto(string Email, string Password, Role Role);