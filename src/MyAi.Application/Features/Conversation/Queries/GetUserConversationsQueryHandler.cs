using MediatR;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Common.Models;
using MyAi.Application.Features.Conversation;

namespace MyAi.Application.Features.Conversation.Queries;

public class GetUserConversationsQueryHandler
    : IRequestHandler<GetUserConversationsQuery, PaginatedList<ConversationDto>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUserConversationsQueryHandler(
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService)
    {
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<ConversationDto>> Handle(GetUserConversationsQuery query, CancellationToken ct)
    {
        var userId = _currentUserService.GetUserId();
        var page = await _conversationRepository.GetUserConversationsAsync(userId, query.Request, ct);

        var items = page.Items
            .Select(c => new ConversationDto(
                c.Id, c.UserId, c.Title, c.MessageCount, c.CreatedAt, c.UpdatedAt))
            .ToList();

        return new PaginatedList<ConversationDto>(items, page.TotalCount, page.PageNumber, page.PageSize);
    }
}
