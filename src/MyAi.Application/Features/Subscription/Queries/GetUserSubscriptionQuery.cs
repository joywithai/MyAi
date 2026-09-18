using MediatR;
using MyAi.Application.Features.Subscription;

namespace MyAi.Application.Features.Subscription.Queries;

public record GetUserSubscriptionQuery : IRequest<UserSubscriptionDto?>;
