namespace MyAi.Domain.Events;

public class SubscriptionCreatedEvent : DomainEvent
{
    public SubscriptionCreatedEvent(Guid userId, Guid planId, DateTime expiresAt)
    {
        UserId = userId;
        PlanId = planId;
        ExpiresAt = expiresAt;
    }

    public Guid UserId { get; }
    public Guid PlanId { get; }
    public DateTime ExpiresAt { get; }
}
