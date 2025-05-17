using CorporateSystem.Auth.Domain.Entities;
using CorporateSystem.Auth.Domain.Enums;

namespace CorporateSystem.Auth.Infrastructure.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
}