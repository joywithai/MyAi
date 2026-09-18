using MyAi.Domain.Events.Base;

namespace MyAi.Application.Common.Interfaces;

/// <summary>Publishes domain events to their handlers.</summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct = default) where TEvent : DomainEvent;
}
