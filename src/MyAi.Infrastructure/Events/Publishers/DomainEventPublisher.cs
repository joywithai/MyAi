using MediatR;
using MyAi.Application.Common.Interfaces;
using MyAi.Domain.Events;
using MyAi.Domain.Events.Base;

namespace MyAi.Infrastructure.Events.Publishers;

/// <summary>Publishes domain events through MediatR's notification pipeline.</summary>
public class DomainEventPublisher : IEventPublisher
{
    private readonly IPublisher _publisher;

    public DomainEventPublisher(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct = default)
        where TEvent : DomainEvent =>
        _publisher.Publish(domainEvent, ct);
}
