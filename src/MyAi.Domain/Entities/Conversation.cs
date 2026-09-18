using MyAi.Domain.Events;
using MyAi.Domain.Events.Base;
using MyAi.Domain.Interfaces;

namespace MyAi.Domain.Entities;

public class Conversation : Entity, IAuditableEntity, ISoftDeletable
{
    private readonly List<Message> _messages = new();

    private Conversation(Guid userId, string? title)
    {
        UserId = userId;
        Title = title;
        MessageCount = 0;
        IsDeleted = false;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }

    public string? Title { get; private set; }

    public int MessageCount { get; private set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

    public static Conversation Create(Guid userId, string? title = null)
    {
        var conversation = new Conversation(userId, title);
        conversation.AddDomainEvent(new ConversationStartedEvent(conversation.Id, userId));
        return conversation;
    }

    public void AddMessage(Message message)
    {
        _messages.Add(message);
        MessageCount++;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new MessageSentEvent(message.Id, Id, UserId, message.Role == Enums.MessageRole.User ? "user" : "assistant"));
    }

    public void UpdateTitle(string title)
    {
        Title = title?.Length > 200 ? title[..200] : title;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
