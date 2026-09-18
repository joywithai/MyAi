using Microsoft.EntityFrameworkCore;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Common.Models;
using MyAi.Domain.Entities;
using MyAi.Infrastructure.Persistence.Repositories.Base;

namespace MyAi.Infrastructure.Persistence.Repositories;

public class MessageRepository : BaseRepository<Message>, IMessageRepository
{
    public MessageRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PaginatedList<Message>> GetByConversationIdAsync(
        Guid conversationId, PagedRequest request, CancellationToken ct = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId);

        var totalCount = await query.CountAsync(ct);

        // Newest first for paging; callers re-order ascending for display.
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PaginatedList<Message>(items, totalCount, request.Page, request.PageSize);
    }

    public async Task<List<Message>> GetRecentByConversationAsync(
        Guid conversationId, int count, CancellationToken ct = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<int> GetDailyCountAsync(Guid userId, CancellationToken ct = default)
    {
        var todayUtc = DateTime.UtcNow.Date;

        return await DbSet
            .AsNoTracking()
            .Where(m => m.Role == Domain.Enums.MessageRole.User && m.CreatedAt >= todayUtc)
            .Join(
                Context.Conversations.IgnoreQueryFilters(),
                m => m.ConversationId,
                c => c.Id,
                (m, c) => new { m, c })
            .Where(x => x.c.UserId == userId)
            .CountAsync(ct);
    }

    public async Task<int> CleanupOldAudioAsync(int olderThanDays, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-olderThanDays);

        var stale = await DbSet
            .Where(m => m.AudioUrl != null && m.CreatedAt < cutoff)
            .ToListAsync(ct);

        foreach (var message in stale)
        {
            message.AttachAudio(Domain.ValueObjects.AudioMetadata.Create(
                string.Empty, 0, string.Empty, new List<Domain.ValueObjects.WordBoundary>()));
        }

        return stale.Count;
    }
}
