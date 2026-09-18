using MediatR;
using Microsoft.Extensions.Logging;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Interfaces;

namespace MyAi.Infrastructure.Events.Handlers;

/// <summary>Invalidates role-dependent caches when a user's role changes.</summary>
public class UserRoleChangedEventHandler : INotificationHandler<UserRoleChangedEvent>
{
    private readonly ICacheService _cacheService;

    private readonly ILogger<UserRoleChangedEventHandler> _logger;

    public UserRoleChangedEventHandler(ICacheService cacheService, ILogger<UserRoleChangedEventHandler> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task Handle(UserRoleChangedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "User {UserId} role changed {OldRole} → {NewRole}; invalidating caches",
            notification.UserId, notification.OldRole, notification.NewRole);

        await _cacheService.RemoveAsync(CacheKeys.Session(notification.UserId), ct);
        await _cacheService.RemoveAsync(CacheKeys.Settings(notification.UserId), ct);
        await _cacheService.RemoveAsync(CacheKeys.FeatureFlags(notification.OldRole), ct);
        await _cacheService.RemoveAsync(CacheKeys.FeatureFlags(notification.NewRole), ct);
    }
}
