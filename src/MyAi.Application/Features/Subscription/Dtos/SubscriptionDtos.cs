namespace MyAi.Application.Features.Subscription;

public record SubscriptionPlanDto(
    Guid Id,
    string Name,
    string RoleGranted,
    string BillingCycle,
    decimal PriceAmount,
    string PriceCurrency,
    string? Description,
    List<string> Features,
    bool IsActive);

public record UserSubscriptionDto(
    Guid Id,
    Guid PlanId,
    string PlanName,
    string Status,
    DateTime StartedAt,
    DateTime ExpiresAt);
