namespace MyAi.Domain.Events;

public class ConversationStartedEvent : DomainEvent
{
    public ConversationStartedEvent(Guid conversationId, Guid userId)
    {
        ConversationId = conversationId;
        UserId = userId;
    }

    public Guid ConversationId { get; }
    public Guid UserId { get; }
}
