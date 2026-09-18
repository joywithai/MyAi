using MyAi.Application.Common.Interfaces;
using MyAi.Domain.Events;
using MyAi.Domain.Events.Base;

namespace MyAi.Infrastructure.Persistence;

/// <summary>
/// Dispatches entity-raised domain events to the publisher.
/// Explicit per-type dispatch keeps everything compile-safe (no reflection).
/// Add a line here when a new domain event type is introduced.
/// </summary>
public static class DomainEventDispatcher
{
    public static async Task DispatchAsync(IEventPublisher publisher, DomainEvent domainEvent, CancellationToken ct)
    {
        switch (domainEvent)
        {
            case UserRegisteredEvent e:
                await publisher.PublishAsync(e, ct);
                break;
            case UserRoleChangedEvent e:
                await publisher.PublishAsync(e, ct);
                break;
            case ConversationStartedEvent e:
                await publisher.PublishAsync(e, ct);
                break;
            case MessageSentEvent e:
                await publisher.PublishAsync(e, ct);
                break;
            case SettingsChangedEvent e:
                await publisher.PublishAsync(e, ct);
                break;
            case SubscriptionCreatedEvent e:
                await publisher.PublishAsync(e, ct);
                break;
            case PaymentCompletedEvent e:
                await publisher.PublishAsync(e, ct);
                break;
            default:
                throw new NotSupportedException(
                    $"Domain event type '{domainEvent.GetType().Name}' has no dispatcher case.");
        }
    }
}
