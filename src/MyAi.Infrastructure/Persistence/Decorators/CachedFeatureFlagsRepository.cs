using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;

namespace MyAi.Infrastructure.Persistence.Decorators;

/// <summary>Decorator: caches role feature flags (feature_flags:{role}, TTL 60 min).</summary>
public class CachedFeatureFlagsRepository : IFeatureFlagsRepository
{
    private readonly FeatureFlagsRepository _inner;
    private readonly ICacheService _cacheService;

    public CachedFeatureFlagsRepository(FeatureFlagsRepository inner, ICacheService cacheService)
    {
        _inner = inner;
        _cacheService = cacheService;
    }

    public async Task<RoleFeatureFlags?> GetByRoleAsync(UserRole role, CancellationToken ct = default) =>
        await _cacheService.GetOrSetAsync(
            CacheKeys.FeatureFlags(RoleConstants.ToRoleString(role)),
            async () => await _inner.GetByRoleAsync(role, ct),
            TimeSpan.FromMinutes(CacheKeys.FeatureFlagsTtlMinutes),
            ct);

    public Task<List<RoleFeatureFlags>> GetAllAsync(CancellationToken ct = default) =>
        _inner.GetAllAsync(ct);

    public async Task UpdateAsync(RoleFeatureFlags flags, CancellationToken ct = default)
    {
        await _inner.UpdateAsync(flags, ct);
        await _cacheService.RemoveAsync(CacheKeys.FeatureFlags(RoleConstants.ToRoleString(flags.Role)), ct);
    }
}
