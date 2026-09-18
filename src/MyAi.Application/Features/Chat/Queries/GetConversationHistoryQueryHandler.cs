using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Common.Models;
using MyAi.Application.Features.Chat;
using MyAi.Domain.ValueObjects;

namespace MyAi.Application.Features.Chat.Queries;

public class GetConversationHistoryQueryHandler
    : IRequestHandler<GetConversationHistoryQuery, PaginatedList<MessageDto>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IFeatureFlagsRepository _featureFlagsRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetConversationHistoryQueryHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IFeatureFlagsRepository featureFlagsRepository,
        ICurrentUserService currentUserService)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _featureFlagsRepository = featureFlagsRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<MessageDto>> Handle(GetConversationHistoryQuery query, CancellationToken ct)
    {
        var userId = _currentUserService.GetUserId();
        var conversation = await _conversationRepository.GetByIdAsync(query.ConversationId, ct);

        if (conversation is null || conversation.IsDeleted)
        {
            throw new NotFoundException(ErrorMessages.ConversationNotFound);
        }

        if (conversation.UserId != userId)
        {
            throw new ForbiddenException(ErrorMessages.ConversationNotOwned);
        }

        var page = await _messageRepository.GetByConversationIdAsync(query.ConversationId, query.Request, ct);

        var items = page.Items
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto(
                m.Id,
                m.ConversationId,
                m.Role == MessageRole.User ? "user" : "assistant",
                m.Content,
                m.Language == Language.Bn ? "bn" : "en",
                m.ExpressionSegments?.Select(s => new ExpressionSegmentDto(s.Expression, s.Text)).ToList(),
                m.AudioUrl,
                m.AudioDurationMs,
                m.AudioContentType,
                m.WordBoundaries?.Select(w => new WordBoundaryDto(
                    w.Word, w.StartTimeMs, w.DurationMs, w.TextOffset, w.WordLength)).ToList(),
                m.AiModelUsed,
                m.CreatedAt))
            .ToList();

        return new PaginatedList<MessageDto>(items, page.TotalCount, page.PageNumber, page.PageSize);
    }
}
