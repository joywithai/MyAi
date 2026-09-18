using AutoMapper;
using MediatR;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Subscription;

namespace MyAi.Application.Features.Subscription.Queries;

public class GetUserSubscriptionQueryHandler : IRequestHandler<GetUserSubscriptionQuery, UserSubscriptionDto?>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetUserSubscriptionQueryHandler(
        ISubscriptionRepository subscriptionRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _subscriptionRepository = subscriptionRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<UserSubscriptionDto?> Handle(GetUserSubscriptionQuery query, CancellationToken ct)
    {
        var userId = _currentUserService.GetUserId();
        var subscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, ct);

        if (subscription is null)
        {
            return null;
        }

        var dto = _mapper.Map<UserSubscriptionDto>(subscription);

        var plan = await _subscriptionRepository.GetPlanByIdAsync(subscription.PlanId, ct);

        return dto with { PlanName = plan?.Name ?? string.Empty };
    }
}
