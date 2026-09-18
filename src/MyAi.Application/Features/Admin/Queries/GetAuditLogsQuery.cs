using MediatR;
using MyAi.Application.Common.Behaviours;
using MyAi.Application.Common.Models;
using MyAi.Application.Features.Admin;
using MyAi.Domain.Enums;

namespace MyAi.Application.Features.Admin.Queries;

public record GetAuditLogsQuery(PagedRequest Request) : IRequest<PaginatedList<AuditLogDto>>, IRequireRole
{
    public UserRole MinimumRole => UserRole.Admin;
}
