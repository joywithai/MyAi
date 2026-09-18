using Microsoft.EntityFrameworkCore;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Common.Models;
using MyAi.Domain.Entities;
using MyAi.Infrastructure.Persistence.Repositories.Base;

namespace MyAi.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PaginatedList<AuditLog>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PaginatedList<AuditLog>(items, totalCount, request.Page, request.PageSize);
    }
}
