namespace MyAi.Application.Common.Interfaces.Repositories;

public interface ISubscriptionRepository
{
    Task<Domain.Entities.UserSubscription?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Domain.Entities.UserSubscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task AddAsync(Domain.Entities.UserSubscription subscription, CancellationToken ct = default);

    Task UpdateAsync(Domain.Entities.UserSubscription subscription, CancellationToken ct = default);

    Task<Domain.Entities.SubscriptionPlan?> GetPlanByIdAsync(Guid planId, CancellationToken ct = default);

    Task<List<Domain.Entities.SubscriptionPlan>> GetActivePlansAsync(CancellationToken ct = default);

    Task<List<Domain.Entities.UserSubscription>> GetActiveExpiredAsync(CancellationToken ct = default);
}
