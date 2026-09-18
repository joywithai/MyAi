using MediatR;
using Microsoft.Extensions.Logging;
using MyAi.Domain.Events;

namespace MyAi.Infrastructure.Events.Handlers;

/// <summary>Updates message analytics/metrics (future: external analytics service).</summary>
public class MessageSentEventHandler : INotificationHandler<MessageSentEvent>
{
    private readonly ILogger<MessageSentEventHandler> _logger;

    public MessageSentEventHandler(ILogger<MessageSentEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(MessageSentEvent notification, CancellationToken ct)
    {
        // Future: push to an analytics pipeline (Prometheus counter / event stream).
        _logger.LogDebug(
            "Message {MessageId} sent in conversation {ConversationId} (role={Role})",
            notification.MessageId, notification.ConversationId, notification.Role);

        return Task.CompletedTask;
    }
}
