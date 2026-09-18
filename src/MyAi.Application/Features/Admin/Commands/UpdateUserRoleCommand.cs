using MediatR;
using MyAi.Application.Common.Behaviours;
using MyAi.Application.Features.Admin;
using MyAi.Domain.Enums;

namespace MyAi.Application.Features.Admin.Commands;

public record UpdateUserRoleCommand(Guid UserId, string Role)
    : IRequest<AdminUserDto>, IRequireRole
{
    public UserRole MinimumRole => UserRole.Admin;
}
