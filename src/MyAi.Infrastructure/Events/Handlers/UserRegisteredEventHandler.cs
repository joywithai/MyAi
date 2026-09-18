using MediatR;
using Microsoft.Extensions.Logging;
using MyAi.Domain.Events;

namespace MyAi.Infrastructure.Events.Handlers;

/// <summary>Sends the welcome email when a user registers.</summary>
public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly IEmailService _emailService;

    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(IEmailService emailService, ILogger<UserRegisteredEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(UserRegisteredEvent notification, CancellationToken ct)
    {
        _logger.LogInformation("User registered: {Email}", notification.Email);

        var body =
            "<html><body style='font-family:Inter,Arial,sans-serif;background:#080808;color:#f0f0f0;padding:32px'>" +
            "<h2 style='color:#a78bfa'>Welcome to MyAi!</h2>" +
            $"<p>Hi {System.Net.WebUtility.HtmlEncode(notification.DisplayName)},</p>" +
            "<p>Your account is ready. Talk to your 3D AI avatar in Bangla or English, " +
            "customize expressions, animations and voice — and much more.</p>" +
            "<p style='color:#888'>— The MyAi Team</p></body></html>";

        await _emailService.SendAsync(notification.Email, "Welcome to MyAi 🎉", body, ct);
    }
}
