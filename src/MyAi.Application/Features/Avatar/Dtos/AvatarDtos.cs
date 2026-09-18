namespace MyAi.Application.Features.Avatar;

public record AvatarModelDto(
    Guid Id,
    string Name,
    string? Description,
    string FileUrl,
    string? ThumbnailUrl,
    string MinRole,
    bool IsDefault,
    bool IsActive);

public record ExpressionDto(
    Guid Id,
    string Name,
    string DisplayName,
    string? Description,
    string MinRole,
    bool IsActive);

public record AnimationDto(
    Guid Id,
    string Name,
    string DisplayName,
    string? Description,
    string MinRole,
    bool IsActive);
