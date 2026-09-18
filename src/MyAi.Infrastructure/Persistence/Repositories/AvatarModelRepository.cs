using Microsoft.EntityFrameworkCore;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Domain.Entities;
using MyAi.Infrastructure.Persistence.Repositories.Base;

namespace MyAi.Infrastructure.Persistence.Repositories;

public class AvatarModelRepository : BaseRepository<AvatarModel>, IAvatarModelRepository
{
    public AvatarModelRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<AvatarModel>> GetActiveAsync(CancellationToken ct = default) =>
        await DbSet.AsNoTracking()
            .Where(m => m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Name)
            .ToListAsync(ct);

    public async Task<List<AvatarModel>> GetAllAsync(CancellationToken ct = default) =>
        await DbSet.AsNoTracking()
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Name)
            .ToListAsync(ct);

    public async Task<AvatarModel?> GetDefaultAsync(CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(m => m.IsDefault && m.IsActive, ct);

    public async Task UnsetAllDefaultsAsync(CancellationToken ct = default)
    {
        await DbSet
            .Where(m => m.IsDefault)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsDefault, false), ct);
    }
}
