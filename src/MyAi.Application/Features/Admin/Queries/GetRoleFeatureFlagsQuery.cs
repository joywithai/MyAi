using MediatR;
using MyAi.Application.Common.Behaviours;
using MyAi.Application.Features.Settings;
using MyAi.Domain.Enums;

namespace MyAi.Application.Features.Admin.Queries;

public record GetRoleFeatureFlagsQuery : IRequest<List<RoleFeatureFlagsDto>>, IRequireRole
{
    public UserRole MinimumRole => UserRole.Admin;
}
