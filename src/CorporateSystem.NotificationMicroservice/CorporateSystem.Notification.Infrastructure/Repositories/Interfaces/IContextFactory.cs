using System.Transactions;
using Microsoft.EntityFrameworkCore;

namespace CorporateSystem.Infrastructure.Repositories.Interfaces;

public interface IContextFactory<out TDbContext> where TDbContext : DbContext
{
    Task<T> ExecuteWithoutCommitAsync<T>(
        Func<TDbContext, Task<T>> action,
        IsolationLevel isolationLevel = IsolationLevel.Snapshot,
        CancellationToken cancellationToken = default);

    Task ExecuteWithCommitAsync(
        Func<TDbContext, Task> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
    
    Task<T> ExecuteWithCommitAsync<T>(
        Func<TDbContext, Task<T>> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
}