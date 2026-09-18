using MediatR;
using MyAi.Application.Common.Behaviours;
using MyAi.Application.Features.Admin;
using MyAi.Domain.Enums;

namespace MyAi.Application.Features.Admin.Commands;

public record UpdateSystemSettingCommand(string Key, string Value)
    : IRequest<SystemSettingDto>, IRequireRole
{
    public UserRole MinimumRole => UserRole.Admin;
}
