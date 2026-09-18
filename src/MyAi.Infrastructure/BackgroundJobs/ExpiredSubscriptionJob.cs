using MyAi.Application.Common.Constants;
using Microsoft.Extensions.Logging;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Domain.Enums;

namespace MyAi.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire recurring job (daily): expires subscriptions past their end date and
/// reverts the user role to public_user, invalidating cached feature flags.
/// </summary>
public class ExpiredSubscriptionJob
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    private readonly IUserRepository _userRepository;

    private readonly ICacheService _cacheService;

    private readonly ILogger<ExpiredSubscriptionJob> _logger;

    public ExpiredSubscriptionJob(
        ISubscriptionRepository subscriptionRepository,
        IUserRepository userRepository,
        ICacheService cacheService,
        ILogger<ExpiredSubscriptionJob> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _userRepository = userRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("ExpiredSubscriptionJob started");

        var expired = await _subscriptionRepository.GetActiveExpiredAsync(ct);

        foreach (var subscription in expired)
        {
            subscription.MarkExpired();
            await _subscriptionRepository.UpdateAsync(subscription, ct);

            var user = await _userRepository.GetByIdAsync(subscription.UserId, ct);

            if (user is { Role: UserRole.Subscriber })
            {
                user.ChangeRole(UserRole.PublicUser);
                await _userRepository.UpdateAsync(user, ct);

                await _cacheService.RemoveAsync(CacheKeys.Settings(user.Id), ct);
                await _cacheService.RemoveAsync(CacheKeys.FeatureFlags(RoleConstants.Subscriber), ct);
                await _cacheService.RemoveAsync(CacheKeys.FeatureFlags(RoleConstants.PublicUser), ct);

                _logger.LogInformation("Subscription expired; user {UserId} reverted to public_user", user.Id);
            }
        }

        _logger.LogInformation("ExpiredSubscriptionJob finished: {Count} subscriptions expired", expired.Count);
    }
}
