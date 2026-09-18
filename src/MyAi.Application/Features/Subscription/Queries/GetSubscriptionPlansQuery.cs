using MediatR;
using MyAi.Application.Common.Behaviours;
using MyAi.Application.Common.Constants;
using MyAi.Application.Features.Subscription;

namespace MyAi.Application.Features.Subscription.Queries;

public record GetSubscriptionPlansQuery : IRequest<List<SubscriptionPlanDto>>, ICacheable
{
    public string CacheKey => CacheKeys.SubscriptionPlansActive();

    public TimeSpan Duration => TimeSpan.FromMinutes(CacheKeys.CatalogTtlMinutes);
}
