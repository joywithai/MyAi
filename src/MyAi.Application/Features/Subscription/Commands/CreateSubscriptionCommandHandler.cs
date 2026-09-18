using AutoMapper;
using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Subscription;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;

namespace MyAi.Application.Features.Subscription.Commands;

public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, UserSubscriptionDto>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public CreateSubscriptionCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        IPaymentRepository paymentRepository,
        IUserRepository userRepository,
        ICacheService cacheService,
        IMapper mapper)
    {
        _subscriptionRepository = subscriptionRepository;
        _paymentRepository = paymentRepository;
        _userRepository = userRepository;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<UserSubscriptionDto> Handle(CreateSubscriptionCommand command, CancellationToken ct)
    {
        var plan = await _subscriptionRepository.GetPlanByIdAsync(command.PlanId, ct)
                   ?? throw new NotFoundException("Plan", command.PlanId);

        var user = await _userRepository.GetByIdAsync(command.UserId, ct)
                   ?? throw new NotFoundException("User", command.UserId);

        var existing = await _subscriptionRepository.GetActiveByUserIdAsync(command.UserId, ct);

        if (existing is not null && existing.IsActive())
        {
            existing.Cancel();
            await _subscriptionRepository.UpdateAsync(existing, ct);
        }

        var now = DateTime.UtcNow;
        var subscription = UserSubscription.Create(command.UserId, plan.Id, now, plan.CalculateExpiryDate(now));
        await _subscriptionRepository.AddAsync(subscription, ct);

        if (user.Role != UserRole.Admin)
        {
            user.ChangeRole(UserRole.Subscriber);
            await _userRepository.UpdateAsync(user, ct);
        }

        var transaction = await _paymentRepository.GetByIdAsync(command.TransactionId, ct);

        if (transaction is not null)
        {
            transaction.AttachSubscription(subscription.Id);
            await _paymentRepository.UpdateAsync(transaction, ct);
        }

        // Role changed → cached flags/settings must be invalidated for the user.
        await _cacheService.RemoveAsync(CacheKeys.FeatureFlags(RoleConstants.ToRoleString(UserRole.Subscriber)), ct);
        await _cacheService.RemoveAsync(CacheKeys.Settings(command.UserId), ct);
        await _cacheService.RemoveAsync(CacheKeys.Session(command.UserId), ct);

        var dto = _mapper.Map<UserSubscriptionDto>(subscription) with { PlanName = plan.Name };
        return dto;
    }
}
