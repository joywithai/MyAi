using Microsoft.EntityFrameworkCore;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;
using MyAi.Infrastructure.Persistence.Repositories.Base;

namespace MyAi.Infrastructure.Persistence.Repositories;

public class ExpressionRepository : BaseRepository<Expression>, IExpressionRepository
{
    public ExpressionRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>Role hierarchy: admin/subscriber see everything; public user sees public entries.</summary>
    public async Task<List<Expression>> GetAccessibleByRoleAsync(UserRole role, CancellationToken ct = default)
    {
        var all = await DbSet.AsNoTracking()
            .Where(e => e.IsActive)
            .OrderBy(e => e.SortOrder)
            .ThenBy(e => e.Name)
            .ToListAsync(ct);

        return all.Where(e => e.IsAccessibleByRole(role)).ToList();
    }

    public async Task<List<Expression>> GetAllActiveAsync(CancellationToken ct = default) =>
        await DbSet.AsNoTracking()
            .Where(e => e.IsActive)
            .OrderBy(e => e.SortOrder)
            .ToListAsync(ct);
}
