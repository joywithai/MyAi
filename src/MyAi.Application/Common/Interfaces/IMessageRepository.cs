namespace MyAi.Application.Common.Interfaces.Repositories;

public interface IMessageRepository
{
    Task<Domain.Entities.Message> AddAsync(Domain.Entities.Message message, CancellationToken ct = default);

    Task<Common.Models.PaginatedList<Domain.Entities.Message>> GetByConversationIdAsync(
        Guid conversationId, Common.Models.PagedRequest request, CancellationToken ct = default);

    Task<List<Domain.Entities.Message>> GetRecentByConversationAsync(
        Guid conversationId, int count, CancellationToken ct = default);

    Task<int> GetDailyCountAsync(Guid userId, CancellationToken ct = default);

    Task<int> CleanupOldAudioAsync(int olderThanDays, CancellationToken ct = default);
}
