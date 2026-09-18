using AutoMapper;
using MediatR;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Subscription;
using MyAi.Application.Features.User;

namespace MyAi.Application.Features.User.Queries;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetUserProfileQueryHandler(
        IUserRepository userRepository,
        ISubscriptionRepository subscriptionRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<UserProfileDto> Handle(GetUserProfileQuery query, CancellationToken ct)
    {
        var userId = _currentUserService.GetUserId();
        var user = await _userRepository.GetByIdAsync(userId, ct)
                   ?? throw new NotFoundException("User", userId);

        var activeSubscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, ct);

        var dto = _mapper.Map<UserProfileDto>(user);
        return dto with
        {
            Subscription = activeSubscription is null
                ? null
                : _mapper.Map<UserSubscriptionDto>(activeSubscription)
        };
    }
}
