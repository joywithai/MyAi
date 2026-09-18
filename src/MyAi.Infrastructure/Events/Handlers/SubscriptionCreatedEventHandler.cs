using MediatR;
using Microsoft.Extensions.Logging;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Domain.Events;

namespace MyAi.Infrastructure.Events.Handlers;

/// <summary>Sends the subscription confirmation email.</summary>
public class SubscriptionCreatedEventHandler : INotificationHandler<SubscriptionCreatedEvent>
{
    private readonly IEmailService _emailService;

    private readonly IUserRepository _userRepository;

    private readonly ILogger<SubscriptionCreatedEventHandler> _logger;

    public SubscriptionCreatedEventHandler(
        IEmailService emailService,
        IUserRepository userRepository,
        ILogger<SubscriptionCreatedEventHandler> logger)
    {
        _emailService = emailService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task Handle(SubscriptionCreatedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "Subscription created for user {UserId}, expires {ExpiresAt}", notification.UserId, notification.ExpiresAt);

        var user = await _userRepository.GetByIdAsync(notification.UserId, ct);

        if (user is null)
        {
            return;
        }

        var body =
            "<html><body style='font-family:Inter,Arial,sans-serif;background:#080808;color:#f0f0f0;padding:32px'>" +
            "<h2 style='color:#a78bfa'>Subscription activated 🎉</h2>" +
            "<p>Your MyAi premium features are now unlocked:</p>" +
            "<ul><li>Custom AI API key</li><li>All 12 expressions &amp; animations</li>" +
            "<li>All avatar models</li><li>Voice speed &amp; pitch control</li>" +
            "<li>Unlimited conversation history</li></ul>" +
            $"<p style='color:#888'>Expires: {notification.ExpiresAt:yyyy-MM-dd}</p></body></html>";

        await _emailService.SendAsync(user.Email.ToString(), "Your MyAi subscription is active", body, ct);
    }
}
