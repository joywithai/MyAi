namespace MyAi.Application.Common.Interfaces;

/// <summary>Ownership probe used by AuthorizeResourceFilter.</summary>
public interface IConversationOwnershipChecker
{
    Task<bool> OwnsConversationAsync(Guid userId, Guid conversationId);
}
