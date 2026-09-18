using System.Text.Json.Serialization;

namespace MyAi.Domain.Events.Base;

/// <summary>Base class for all domain events.</summary>
public abstract class DomainEvent
{
    [JsonIgnore]
    public Guid EventId { get; } = Guid.NewGuid();

    [JsonIgnore]
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    [JsonIgnore]
    public string EventType => GetType().Name;
}
