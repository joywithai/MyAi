using MediatR;
using MyAi.Application.Common.Behaviours;
using MyAi.Application.Features.Avatar;
using MyAi.Domain.Enums;

namespace MyAi.Application.Features.Admin.Commands;

public record CreateAvatarModelCommand(
    string Name,
    string FileUrl,
    string? ThumbnailUrl,
    string MinRole,
    string? Description,
    bool IsDefault) : IRequest<AvatarModelDto>, IRequireRole
{
    public UserRole MinimumRole => UserRole.Admin;
}

public record UpdateAvatarModelCommand(
    Guid Id,
    string? Name = null,
    string? Description = null,
    string? ThumbnailUrl = null,
    string? MinRole = null,
    bool? IsDefault = null,
    bool? IsActive = null) : IRequest<AvatarModelDto>, IRequireRole
{
    public UserRole MinimumRole => UserRole.Admin;
}

public record DeactivateAvatarModelCommand(Guid Id) : IRequest<bool>, IRequireRole
{
    public UserRole MinimumRole => UserRole.Admin;
}
