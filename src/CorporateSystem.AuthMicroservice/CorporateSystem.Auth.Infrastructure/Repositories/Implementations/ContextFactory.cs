using System.Runtime.CompilerServices;
using System.Transactions;
using CorporateSystem.Auth.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("CorporateSystem.Auth.Tests")]

namespace CorporateSystem.Auth.Infrastructure.Repositories.Implementations;

internal sealed class ContextFactory<TDbContext>(DbContextOptions<TDbContext> options) : IContextFactory<TDbContext>
    where TDbContext : DbContext
{
    public async Task<T> ExecuteWithoutCommitAsync<T>(
        Func<TDbContext, Task<T>> action,
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(action);
        
        cancellationToken.ThrowIfCancellationRequested();
        await using var context = (TDbContext)Activator.CreateInstance(typeof(TDbContext), options)!;
        
        using var scope = CreateTransactionScope(isolationLevel);

        var result = await action(context);
        scope.Complete();

        return result;
    }

    public async Task ExecuteWithCommitAsync(
        Func<TDbContext, Task> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        await using var context = (TDbContext)Activator.CreateInstance(typeof(TDbContext), options)!;
        
        using var scope = CreateTransactionScope(isolationLevel);

        await action(context);
            
        await context.SaveChangesAsync(cancellationToken);
        scope.Complete();
    }

    public async Task<T> ExecuteWithCommitAsync<T>(
        Func<TDbContext, Task<T>> action,
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(action);
        await using var context = (TDbContext)Activator.CreateInstance(typeof(TDbContext), options)!;

        using var scope = CreateTransactionScope(isolationLevel);

        var result = await action(context);
            
        await context.SaveChangesAsync(cancellationToken);
        scope.Complete();
            
        return result;
    }
    
    private TransactionScope CreateTransactionScope(IsolationLevel isolationLevel) => 
        new(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = isolationLevel },
            TransactionScopeAsyncFlowOption.Enabled);
}