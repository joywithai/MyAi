using Microsoft.EntityFrameworkCore;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Domain.Entities;
using MyAi.Infrastructure.Persistence.Repositories.Base;

namespace MyAi.Infrastructure.Persistence.Repositories;

public class SubscriptionRepository : BaseRepository<UserSubscription>, ISubscriptionRepository
{
    public SubscriptionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<UserSubscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        return await DbSet
            .Where(s => s.UserId == userId
                        && s.Status == Domain.Enums.SubscriptionStatus.Active
                        && s.ExpiresAt > now)
            .OrderByDescending(s => s.ExpiresAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<SubscriptionPlan?> GetPlanByIdAsync(Guid planId, CancellationToken ct = default) =>
        await Context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == planId, ct);

    public async Task<List<SubscriptionPlan>> GetActivePlansAsync(CancellationToken ct = default) =>
        await Context.SubscriptionPlans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);

    public async Task<List<UserSubscription>> GetActiveExpiredAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        return await DbSet
            .Where(s => s.Status == Domain.Enums.SubscriptionStatus.Active && s.ExpiresAt <= now)
            .ToListAsync(ct);
    }
}
