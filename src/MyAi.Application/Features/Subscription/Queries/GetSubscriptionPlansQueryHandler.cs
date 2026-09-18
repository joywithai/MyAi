using AutoMapper;
using MediatR;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Subscription;

namespace MyAi.Application.Features.Subscription.Queries;

public class GetSubscriptionPlansQueryHandler
    : IRequestHandler<GetSubscriptionPlansQuery, List<SubscriptionPlanDto>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMapper _mapper;

    public GetSubscriptionPlansQueryHandler(ISubscriptionRepository subscriptionRepository, IMapper mapper)
    {
        _subscriptionRepository = subscriptionRepository;
        _mapper = mapper;
    }

    public async Task<List<SubscriptionPlanDto>> Handle(GetSubscriptionPlansQuery query, CancellationToken ct)
    {
        var plans = await _subscriptionRepository.GetActivePlansAsync(ct);
        return plans.Select(_mapper.Map<SubscriptionPlanDto>).ToList();
    }
}
