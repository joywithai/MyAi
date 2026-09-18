using MediatR;
using MyAi.Application.Common.Behaviours;
using MyAi.Application.Common.Models;
using MyAi.Application.Features.Admin;
using MyAi.Domain.Enums;

namespace MyAi.Application.Features.Admin.Queries;

public record GetAllUsersQuery(
    PagedRequest Request,
    string? Role = null,
    string? Status = null) : IRequest<PaginatedList<AdminUserDto>>, IRequireRole
{
    public UserRole MinimumRole => UserRole.Admin;
}
