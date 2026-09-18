namespace MyAi.Application.Common.Interfaces.Repositories;

public interface IConversationRepository
{
    Task<Domain.Entities.Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(Domain.Entities.Conversation conversation, CancellationToken ct = default);

    Task UpdateAsync(Domain.Entities.Conversation conversation, CancellationToken ct = default);

    Task<Common.Models.PaginatedList<Domain.Entities.Conversation>> GetUserConversationsAsync(
        Guid userId, Common.Models.PagedRequest request, CancellationToken ct = default);
}
