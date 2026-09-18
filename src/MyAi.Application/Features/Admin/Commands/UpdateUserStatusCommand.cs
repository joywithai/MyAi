using MediatR;
using MyAi.Application.Common.Behaviours;
using MyAi.Application.Features.Admin;
using MyAi.Domain.Enums;

namespace MyAi.Application.Features.Admin.Commands;

public record UpdateUserStatusCommand(Guid UserId, string Status)
    : IRequest<AdminUserDto>, IRequireRole
{
    public UserRole MinimumRole => UserRole.Admin;
}
