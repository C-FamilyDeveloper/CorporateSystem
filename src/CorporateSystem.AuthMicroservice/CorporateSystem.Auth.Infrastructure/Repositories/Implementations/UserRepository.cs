using CorporateSystem.Auth.Domain.Entities;
using CorporateSystem.Auth.Infrastructure.Extensions;
using CorporateSystem.Auth.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CorporateSystem.Auth.Infrastructure.Repositories.Implementations;

internal class UserRepository(IContextFactory contextFactory, ILogger<UserRepository> logger) : IUserRepository
{
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            logger.LogError($"{nameof(GetUserByEmailAsync)}: email is null or white space");
            throw new ArgumentException("Некорректный email");
        }

        return await contextFactory.ExecuteWithoutCommitAsync(
            async context => await context.Users.FirstOrDefaultAsync(user => user.Email == email, cancellationToken),
            cancellationToken: cancellationToken);
    }
}