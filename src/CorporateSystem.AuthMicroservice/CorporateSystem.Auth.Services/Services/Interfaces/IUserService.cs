using CorporateSystem.Auth.Domain.Entities;

namespace CorporateSystem.Auth.Services.Services.Interfaces;

public interface IUserService
{
    Task<User[]> GetUsersByIdsAsync(int[] ids, CancellationToken cancellationToken = default);
    Task<User[]> GetUsersByEmailsAsync(string[] emails, CancellationToken cancellationToken = default);
}