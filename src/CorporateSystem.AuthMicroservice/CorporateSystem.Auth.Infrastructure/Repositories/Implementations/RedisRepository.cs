using CorporateSystem.Auth.Infrastructure.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CorporateSystem.Auth.Infrastructure.Repositories.Implementations;

internal abstract class RedisRepository(RedisOptions redisOptions)
{
    private static ConnectionMultiplexer? _connection;
    
    protected abstract string KeyPrefix { get; }
    
    
    protected async Task<IDatabase> GetConnectionAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        _connection ??= await ConnectionMultiplexer.ConnectAsync(redisOptions.ConnectionString);
        
        return _connection.GetDatabase();
    }
    
    protected RedisKey GetKey(params object[] keys)
        => new($"{KeyPrefix}:{string.Join(":", keys)}");
}