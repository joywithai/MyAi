using MediatR;
using Microsoft.Extensions.Logging;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Domain.Entities;
using MyAi.Domain.Events;

namespace MyAi.Infrastructure.Events.Handlers;

/// <summary>Sends the payment receipt email and writes an audit log entry.</summary>
public class PaymentCompletedEventHandler : INotificationHandler<PaymentCompletedEvent>
{
    private readonly IEmailService _emailService;

    private readonly IAuditLogRepository _auditLogRepository;

    private readonly IUserRepository _userRepository;

    private readonly ILogger<PaymentCompletedEventHandler> _logger;

    public PaymentCompletedEventHandler(
        IEmailService emailService,
        IAuditLogRepository auditLogRepository,
        IUserRepository userRepository,
        ILogger<PaymentCompletedEventHandler> logger)
    {
        _emailService = emailService;
        _auditLogRepository = auditLogRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task Handle(PaymentCompletedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "Payment {TransactionId} completed: {Amount} {Provider}",
            notification.TransactionId, notification.Amount, notification.Provider);

        var user = await _userRepository.GetByIdAsync(notification.UserId, ct);

        await _auditLogRepository.AddAsync(
            AuditLog.Create(
                notification.UserId,
                "payment.completed_event",
                "payment_transaction",
                notification.TransactionId,
                null,
                new { notification.Amount, notification.Provider, PiiAccessed = user is not null }),
            ct);

        if (user is null)
        {
            return;
        }

        var body =
            "<html><body style='font-family:Inter,Arial,sans-serif;background:#080808;color:#f0f0f0;padding:32px'>" +
            "<h2 style='color:#a78bfa'>Payment receipt</h2>" +
            $"<p>Amount: <b>{notification.Amount:F2} BDT</b></p>" +
            $"<p>Provider: {System.Net.WebUtility.HtmlEncode(notification.Provider)}</p>" +
            $"<p>Transaction: {notification.TransactionId}</p>" +
            "<p style='color:#888'>Thank you for supporting MyAi!</p></body></html>";

        await _emailService.SendAsync(user.Email.ToString(), "Your MyAi payment receipt", body, ct);
    }
}
