using MediatR;
using MyAi.Application.Common.Behaviours;
using MyAi.Application.Features.Admin;
using MyAi.Domain.Enums;

namespace MyAi.Application.Features.Admin.Queries;

public record GetAllSystemSettingsQuery : IRequest<List<SystemSettingDto>>, IRequireRole
{
    public UserRole MinimumRole => UserRole.Admin;
}
