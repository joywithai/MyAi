namespace MyAi.Domain.Events;

public class UserRegisteredEvent : DomainEvent
{
    public UserRegisteredEvent(Guid userId, string email, string displayName)
    {
        UserId = userId;
        Email = email;
        DisplayName = displayName;
    }

    public Guid UserId { get; }
    public string Email { get; }
    public string DisplayName { get; }
}
