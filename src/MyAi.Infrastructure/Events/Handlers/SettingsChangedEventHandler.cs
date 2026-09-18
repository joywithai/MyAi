using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Interfaces;
using MyAi.Domain.Events;

namespace MyAi.Infrastructure.Events.Handlers;

/// <summary>Invalidates the user's settings cache (settings:{userId}) on change.</summary>
public class SettingsChangedEventHandler : INotificationHandler<SettingsChangedEvent>
{
    private readonly ICacheService _cacheService;

    public SettingsChangedEventHandler(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task Handle(SettingsChangedEvent notification, CancellationToken ct)
    {
        await _cacheService.RemoveAsync(CacheKeys.Settings(notification.UserId), ct);
    }
}
