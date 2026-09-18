namespace MyAi.Infrastructure.Caching.Redis;

/// <summary>Strongly typed Redis options bound from ConnectionStrings:Redis.</summary>
public class RedisConfiguration
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = string.Empty;

    public string InstanceName { get; set; } = "myai:";

    public bool Enabled => !string.IsNullOrWhiteSpace(ConnectionString);
}
