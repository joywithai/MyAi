using Microsoft.EntityFrameworkCore;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;
using MyAi.Infrastructure.Persistence.Repositories.Base;

namespace MyAi.Infrastructure.Persistence.Repositories;

public class AnimationRepository : BaseRepository<Animation>, IAnimationRepository
{
    public AnimationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<Animation>> GetAccessibleByRoleAsync(UserRole role, CancellationToken ct = default)
    {
        var all = await DbSet.AsNoTracking()
            .Where(a => a.IsActive)
            .OrderBy(a => a.SortOrder)
            .ToListAsync(ct);

        return all.Where(a => a.IsAccessibleByRole(role)).ToList();
    }

    public async Task<List<Animation>> GetAllActiveAsync(CancellationToken ct = default) =>
        await DbSet.AsNoTracking()
            .Where(a => a.IsActive)
            .OrderBy(a => a.SortOrder)
            .ToListAsync(ct);
}
