using MediatR;
using MyAi.Application.Common.Behaviours;
using MyAi.Application.Features.Avatar;
using MyAi.Domain.Enums;

namespace MyAi.Application.Features.Admin.Commands;

public record CreateExpressionCommand(
    string Name,
    string DisplayName,
    string? Description,
    string MinRole) : IRequest<ExpressionDto>, IRequireRole
{
    public UserRole MinimumRole => UserRole.Admin;
}

public record UpdateExpressionCommand(
    Guid Id,
    string? DisplayName = null,
    string? Description = null,
    string? MinRole = null,
    bool? IsActive = null) : IRequest<ExpressionDto>, IRequireRole
{
    public UserRole MinimumRole => UserRole.Admin;
}

public record CreateAnimationCommand(
    string Name,
    string DisplayName,
    string? Description,
    string MinRole) : IRequest<AnimationDto>, IRequireRole
{
    public UserRole MinimumRole => UserRole.Admin;
}

public record UpdateAnimationCommand(
    Guid Id,
    string? DisplayName = null,
    string? Description = null,
    string? MinRole = null,
    bool? IsActive = null) : IRequest<AnimationDto>, IRequireRole
{
    public UserRole MinimumRole => UserRole.Admin;
}
