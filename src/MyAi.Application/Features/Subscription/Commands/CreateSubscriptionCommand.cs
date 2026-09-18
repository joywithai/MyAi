using MediatR;
using MyAi.Application.Features.Subscription;

namespace MyAi.Application.Features.Subscription.Commands;

/// <summary>Internal command dispatched by the payment flow once payment succeeds.</summary>
public record CreateSubscriptionCommand(Guid UserId, Guid PlanId, Guid TransactionId)
    : IRequest<UserSubscriptionDto>;
