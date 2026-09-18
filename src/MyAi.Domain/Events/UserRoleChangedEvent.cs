namespace MyAi.Domain.Events;

public class UserRoleChangedEvent : DomainEvent
{
    public UserRoleChangedEvent(Guid userId, string oldRole, string newRole)
    {
        UserId = userId;
        OldRole = oldRole;
        NewRole = newRole;
    }

    public Guid UserId { get; }
    public string OldRole { get; }
    public string NewRole { get; }
}
