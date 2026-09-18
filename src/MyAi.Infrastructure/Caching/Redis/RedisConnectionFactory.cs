using StackExchange.Redis;

namespace MyAi.Infrastructure.Caching.Redis;

/// <summary>Lazy singleton ConnectionMultiplexer factory.</summary>
public class RedisConnectionFactory
{
    private readonly RedisConfiguration _configuration;

    private readonly Lazy<ConnectionMultiplexer> _connection;

    public RedisConnectionFactory(RedisConfiguration configuration)
    {
        _configuration = configuration;
        _connection = new Lazy<ConnectionMultiplexer>(() =>
            ConnectionMultiplexer.Connect(_configuration.ConnectionString));
    }

    public IConnectionMultiplexer GetConnection() => _connection.Value;

    public IDatabase GetDatabase(int db = -1) => GetConnection().GetDatabase(db);
}
