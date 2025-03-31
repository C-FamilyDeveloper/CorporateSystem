using CorporateSystem.Auth.Domain.Entities;
using CorporateSystem.Auth.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CorporateSystem.Auth.Infrastructure.Repositories.Implementations;

internal class UserRepository(IContextFactory contextFactory) : IUserRepository
{
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return await contextFactory.ExecuteWithoutCommitAsync(
            async context => await context.Users.FirstOrDefaultAsync(user => user.Email == email, cancellationToken),
            cancellationToken: cancellationToken);
    }

    public async Task AddUserAsync(AddUserDto dto, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dto.Email);
        ArgumentException.ThrowIfNullOrWhiteSpace(dto.Password);
        
        await contextFactory.ExecuteWithCommitAsync(
            async context =>
                await context.Users.AddAsync(
                    new User
                    {
                        Email = dto.Email,
                        Password = dto.Password,
                        Role = dto.Role
                    }, cancellationToken),
            cancellationToken: cancellationToken);
    }
}