using System.Transactions;

namespace CorporateSystem.Auth.Infrastructure.Repositories.Interfaces;

public interface IContextFactory
{
    Task<T> ExecuteWithoutCommitAsync<T>(
        Func<DataContext, Task<T>> action,
        IsolationLevel isolationLevel = IsolationLevel.Snapshot,
        CancellationToken cancellationToken = default);

    Task ExecuteWithCommitAsync(
        Func<DataContext, Task> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
    
    Task<T> ExecuteWithCommitAsync<T>(
        Func<DataContext, Task<T>> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
}