namespace MyAi.Application.Common.Interfaces.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(Domain.Entities.AuditLog auditLog, CancellationToken ct = default);

    Task<Common.Models.PaginatedList<Domain.Entities.AuditLog>> GetPagedAsync(
        Common.Models.PagedRequest request, CancellationToken ct = default);
}
