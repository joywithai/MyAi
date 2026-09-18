using Microsoft.EntityFrameworkCore;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Common.Models;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;
using MyAi.Infrastructure.Persistence.Repositories.Base;

namespace MyAi.Infrastructure.Persistence.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await DbSet.FirstOrDefaultAsync(u => u.Email == Domain.ValueObjects.Email.Create(normalized), ct);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await DbSet.AnyAsync(u => u.Email == Domain.ValueObjects.Email.Create(normalized), ct);
    }

    public async Task UpdateRoleAsync(Guid userId, UserRole role, CancellationToken ct = default)
    {
        await DbSet
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.Role, role)
                .SetProperty(u => u.UpdatedAt, DateTime.UtcNow), ct);
    }

    public async Task UpdateStatusAsync(Guid userId, UserStatus status, CancellationToken ct = default)
    {
        await DbSet
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.Status, status)
                .SetProperty(u => u.UpdatedAt, DateTime.UtcNow), ct);
    }

    public async Task<RefreshToken?> FindRefreshTokenAsync(string token, CancellationToken ct = default) =>
        await Context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token, ct);

    public async Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default)
    {
        await Context.RefreshTokens.AddAsync(token, ct);
        await Context.SaveChangesAsync(ct);
    }

    public async Task<PaginatedList<User>> GetPagedAsync(
        PagedRequest request, UserRole? role, UserStatus? status, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(u => u.Status == status.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PaginatedList<User>(items, totalCount, request.Page, request.PageSize);
    }

    public async Task<int> CleanupExpiredRefreshTokensAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var stale = await Context.RefreshTokens
            .Where(t => t.ExpiresAt < now.AddDays(-7) || (t.IsRevoked && t.RevokedAt < now.AddDays(-7)))
            .ToListAsync(ct);

        Context.RefreshTokens.RemoveRange(stale);
        return stale.Count;
    }
}
