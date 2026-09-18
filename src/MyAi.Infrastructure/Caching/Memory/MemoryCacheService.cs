using Microsoft.Extensions.Caching.Memory;
using MyAi.Application.Common.Interfaces;

namespace MyAi.Infrastructure.Caching.Memory;

/// <summary>
/// In-memory ICacheService. Fallback for development / when Redis is not configured.
/// Graceful degradation: cache failure never breaks the request path.
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        return Task.FromResult(_memoryCache.TryGetValue(key, out T? value) ? value : null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default) where T : class
    {
        var options = new MemoryCacheEntryOptions();

        if (ttl.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = ttl.Value;
        }

        _memoryCache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _memoryCache.Remove(key);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        return Task.FromResult(_memoryCache.TryGetValue(key, out _));
    }

    public async Task<T> GetOrSetAsync<T>(
        string key, Func<Task<T>> factory, TimeSpan? ttl = null, CancellationToken ct = default) where T : class
    {
        if (_memoryCache.TryGetValue(key, out T? cached) && cached is not null)
        {
            return cached;
        }

        var value = await factory();
        await SetAsync(key, value, ttl, ct);
        return value;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken ct = default)
    {
        if (_memoryCache is MemoryCache concreteCache)
        {
            var keys = concreteCache.GetKeys<string>().Where(k => MatchesPattern(k, pattern)).ToList();
            foreach (var key in keys)
            {
                _memoryCache.Remove(key);
            }
        }

        return Task.CompletedTask;
    }

    private static bool MatchesPattern(string key, string pattern)
    {
        if (pattern.EndsWith('*'))
        {
            return key.StartsWith(pattern[..^1], StringComparison.Ordinal);
        }

        return key == pattern;
    }
}
