using CorporateSystem.Auth.Domain.Entities;
using CorporateSystem.Auth.Services.Services.Filters;

namespace CorporateSystem.Auth.Services.Services.Interfaces;

public interface IUserService
{
    Task<User[]> GetUsersByFilterAsync(UserFilter filter, CancellationToken cancellationToken = default);
}