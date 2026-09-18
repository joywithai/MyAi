using Microsoft.EntityFrameworkCore;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;

namespace MyAi.Infrastructure.Persistence.Repositories;

public class FeatureFlagsRepository : IFeatureFlagsRepository
{
    private readonly ApplicationDbContext _context;

    public FeatureFlagsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RoleFeatureFlags?> GetByRoleAsync(UserRole role, CancellationToken ct = default) =>
        await _context.RoleFeatureFlags.FirstOrDefaultAsync(f => f.Role == role, ct);

    public async Task<List<RoleFeatureFlags>> GetAllAsync(CancellationToken ct = default) =>
        await _context.RoleFeatureFlags.AsNoTracking().ToListAsync(ct);

    public async Task UpdateAsync(RoleFeatureFlags flags, CancellationToken ct = default)
    {
        _context.RoleFeatureFlags.Update(flags);
        await _context.SaveChangesAsync(ct);
    }
}
