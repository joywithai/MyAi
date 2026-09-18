using Microsoft.EntityFrameworkCore;
using MyAi.Application.Common.Interfaces;

namespace MyAi.Infrastructure.Persistence;

/// <summary>EF-backed ownership check for the AuthorizeResourceFilter.</summary>
public class ConversationOwnershipChecker : IConversationOwnershipChecker
{
    private readonly ApplicationDbContext _context;

    public ConversationOwnershipChecker(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<bool> OwnsConversationAsync(Guid userId, Guid conversationId) =>
        _context.Conversations
            .AsNoTracking()
            .AnyAsync(c => c.Id == conversationId && c.UserId == userId);
}
