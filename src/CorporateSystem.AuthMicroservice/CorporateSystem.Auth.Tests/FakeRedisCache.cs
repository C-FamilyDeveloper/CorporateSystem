using Microsoft.Extensions.Caching.Distributed;

namespace CorporateSystem.Auth.Tests;

public class FakeRedisCache : IDistributedCache
{
    private readonly Dictionary<string, byte[]> _cache = new();
    
    public byte[]? Get(string key)
    {
        _cache.TryGetValue(key, out var result);

        return result;
    }

    public Task<byte[]?> GetAsync(string key, CancellationToken token = new CancellationToken())
    {
        _cache.TryGetValue(key, out var result);

        return Task.FromResult(result);
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        _cache[key] = value;
    }

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
        CancellationToken token = new CancellationToken())
    {
        _cache[key] = value;

        return Task.CompletedTask;
    }

    public void Refresh(string key)
    {
        throw new NotImplementedException();
    }

    public Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }

    public Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
    {
        return Task.FromResult(_cache.Remove(key));
    }
}