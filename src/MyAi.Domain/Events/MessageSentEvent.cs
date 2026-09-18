namespace MyAi.Domain.Events;

public class MessageSentEvent : DomainEvent
{
    public MessageSentEvent(Guid messageId, Guid conversationId, Guid userId, string role)
    {
        MessageId = messageId;
        ConversationId = conversationId;
        UserId = userId;
        Role = role;
    }

    public Guid MessageId { get; }
    public Guid ConversationId { get; }
    public Guid UserId { get; }
    public string Role { get; }
}
