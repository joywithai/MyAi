using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Common.Models;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;

namespace MyAi.Infrastructure.Persistence.Decorators;

/// <summary>Decorator: caches user lookups (user:{id}, 5 min TTL) and invalidates on update.</summary>
public class CachedUserRepository : IUserRepository
{
    private readonly UserRepository _inner;
    private readonly ICacheService _cacheService;

    public CachedUserRepository(UserRepository inner, ICacheService cacheService)
    {
        _inner = inner;
        _cacheService = cacheService;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _cacheService.GetOrSetAsync(
            CacheKeys.User(id),
            async () => await _inner.GetByIdAsync(id, ct),
            TimeSpan.FromMinutes(5),
            ct);

    public Task<User?> FindByEmailAsync(string email, CancellationToken ct = default) =>
        _inner.FindByEmailAsync(email, ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) =>
        _inner.ExistsByEmailAsync(email, ct);

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await _inner.AddAsync(user, ct);

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        await _inner.UpdateAsync(user, ct);
        await _cacheService.RemoveAsync(CacheKeys.User(user.Id), ct);
    }

    public Task UpdateRoleAsync(Guid userId, UserRole role, CancellationToken ct = default) =>
        _inner.UpdateRoleAsync(userId, role, ct);

    public Task UpdateStatusAsync(Guid userId, UserStatus status, CancellationToken ct = default) =>
        _inner.UpdateStatusAsync(userId, status, ct);

    public Task<RefreshToken?> FindRefreshTokenAsync(string token, CancellationToken ct = default) =>
        _inner.FindRefreshTokenAsync(token, ct);

    public Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default) =>
        _inner.AddRefreshTokenAsync(token, ct);

    public Task<PaginatedList<User>> GetPagedAsync(
        PagedRequest request, UserRole? role, UserStatus? status, CancellationToken ct = default) =>
        _inner.GetPagedAsync(request, role, status, ct);

    public Task<int> CleanupExpiredRefreshTokensAsync(CancellationToken ct = default) =>
        _inner.CleanupExpiredRefreshTokensAsync(ct);
}
