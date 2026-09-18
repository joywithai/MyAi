using System.Text.Json;
using Microsoft.Extensions.Logging;
using MyAi.Application.Common.Interfaces;
using StackExchange.Redis;

namespace MyAi.Infrastructure.Caching.Redis;

/// <summary>Redis-backed ICacheService implementation.</summary>
public class RedisCacheService : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RedisConnectionFactory _connectionFactory;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(RedisConnectionFactory connectionFactory, ILogger<RedisCacheService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        try
        {
            var value = await _connectionFactory.GetDatabase().StringGetAsync(key);

            return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<T>(value!, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis GET failed for key {Key}; treating as cache miss", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default) where T : class
    {
        try
        {
            var serialized = JsonSerializer.Serialize(value, JsonOptions);
            await _connectionFactory.GetDatabase().StringSetAsync(key, serialized, ttl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis SET failed for key {Key}; skipping cache write", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _connectionFactory.GetDatabase().KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis DELETE failed for key {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        try
        {
            return await _connectionFactory.GetDatabase().KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis EXISTS failed for key {Key}", key);
            return false;
        }
    }

    public async Task<T> GetOrSetAsync<T>(
        string key, Func<Task<T>> factory, TimeSpan? ttl = null, CancellationToken ct = default) where T : class
    {
        var cached = await GetAsync<T>(key, ct);

        if (cached is not null)
        {
            return cached;
        }

        var value = await factory();
        await SetAsync(key, value, ttl, ct);
        return value;
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken ct = default)
    {
        try
        {
            var database = _connectionFactory.GetDatabase();
            var server = _connectionFactory.GetConnection().GetServers().FirstOrDefault();

            if (server is null || !server.IsConnected)
            {
                return;
            }

            await foreach (var key in server.KeysAsync(database.Database, pattern, 250))
            {
                await database.KeyDeleteAsync(key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis pattern delete failed for {Pattern}", pattern);
        }
    }
}
