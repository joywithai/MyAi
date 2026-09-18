namespace MyAi.Application.Features.Admin;

public record AdminUserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string Role,
    string Status,
    bool EmailVerified,
    DateTime CreatedAt,
    DateTime? LastLoginAt);

public record SystemSettingDto(string Key, string Value, string? Description, DateTime UpdatedAt);

public record AuditLogDto(
    Guid Id,
    Guid? UserId,
    string Action,
    string? EntityType,
    Guid? EntityId,
    string? IpAddress,
    DateTime CreatedAt);
