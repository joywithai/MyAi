using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Common.Models;
using MyAi.Domain.Entities;

namespace MyAi.Infrastructure.Persistence.Decorators;

/// <summary>Decorator (Proxy pattern): adds cache-aside behaviour to ConversationRepository.</summary>
public class CachedConversationRepository : IConversationRepository
{
    private readonly ConversationRepository _inner;
    private readonly ICacheService _cacheService;

    public CachedConversationRepository(ConversationRepository inner, ICacheService cacheService)
    {
        _inner = inner;
        _cacheService = cacheService;
    }

    public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _inner.GetByIdAsync(id, ct);

    public async Task AddAsync(Conversation conversation, CancellationToken ct = default)
    {
        await _inner.AddAsync(conversation, ct);
    }

    public async Task UpdateAsync(Conversation conversation, CancellationToken ct = default)
    {
        await _inner.UpdateAsync(conversation, ct);
        await _cacheService.RemoveAsync(CacheKeys.ConversationsPage(conversation.UserId, 1), ct);
    }

    public async Task<PaginatedList<Conversation>> GetUserConversationsAsync(
        Guid userId, PagedRequest request, CancellationToken ct = default)
    {
        var cacheKey = CacheKeys.ConversationsPage(userId, request.Page);

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            () => _inner.GetUserConversationsAsync(userId, request, ct),
            TimeSpan.FromMinutes(CacheKeys.ConversationsTtlMinutes),
            ct);
    }
}
