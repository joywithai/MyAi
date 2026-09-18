using MyAi.Domain.Enums;
using MyAi.Domain.Events;
using MyAi.Domain.Events.Base;
using MyAi.Domain.Interfaces;

namespace MyAi.Domain.Entities;

public class UserSubscription : Entity, IAuditableEntity
{
    private UserSubscription() { } // EF Core

    private UserSubscription(Guid userId, Guid planId, DateTime startedAt, DateTime expiresAt)
    {
        UserId = userId;
        PlanId = planId;
        Status = SubscriptionStatus.Active;
        StartedAt = startedAt;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SubscriptionCreatedEvent(userId, planId, expiresAt));
    }

    public Guid UserId { get; private set; }

    public Guid PlanId { get; private set; }

    public SubscriptionStatus Status { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime? CancelledAt { get; private set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public static UserSubscription Create(Guid userId, Guid planId, DateTime startDate, DateTime expiresAt)
    {
        if (expiresAt <= startDate)
        {
            throw new ArgumentException("Subscription expiry must be after start date.");
        }

        return new UserSubscription(userId, planId, startDate, expiresAt);
    }

    public void Cancel()
    {
        Status = SubscriptionStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkExpired()
    {
        Status = SubscriptionStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsActive() => Status == SubscriptionStatus.Active && ExpiresAt > DateTime.UtcNow;
}
