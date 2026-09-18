namespace MyAi.Application.Common.Interfaces;

/// <summary>Cache abstraction (Redis in production, in-memory fallback for development).</summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;

    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default) where T : class;

    Task RemoveAsync(string key, CancellationToken ct = default);

    Task<bool> ExistsAsync(string key, CancellationToken ct = default);

    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null, CancellationToken ct = default) where T : class;

    Task RemoveByPatternAsync(string pattern, CancellationToken ct = default);
}
