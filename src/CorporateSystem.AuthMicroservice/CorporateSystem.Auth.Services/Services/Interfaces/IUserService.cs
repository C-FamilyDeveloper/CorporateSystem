using System.Linq.Expressions;
using CorporateSystem.Auth.Domain.Entities;

namespace CorporateSystem.Auth.Services.Services.Interfaces;

public interface IUserService
{
    Task<User[]> GetUsersByExpressionAsync(
        Expression<Func<User, bool>> expression,
        CancellationToken cancellationToken = default);
    
    Task DeleteUsersAsync(int[] ids, CancellationToken cancellationToken = default);
}