namespace MyAi.Application.Common.Behaviours;

/// <summary>Marker interface: requests implementing this are cached by CachingBehaviour.</summary>
public interface ICacheable
{
    string CacheKey { get; }

    TimeSpan Duration { get; }
}
