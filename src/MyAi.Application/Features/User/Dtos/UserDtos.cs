using MyAi.Application.Features.Subscription;

namespace MyAi.Application.Features.User;

public record UserProfileDto(
    Guid Id,
    string Email,
    string DisplayName,
    string Role,
    string Status,
    bool EmailVerified,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    UserSubscriptionDto? Subscription);
