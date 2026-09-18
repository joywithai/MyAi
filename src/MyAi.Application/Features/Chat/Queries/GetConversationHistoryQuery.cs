using MediatR;
using MyAi.Application.Common.Models;
using MyAi.Application.Features.Chat;

namespace MyAi.Application.Features.Chat.Queries;

public record GetConversationHistoryQuery(Guid ConversationId, PagedRequest Request)
    : IRequest<PaginatedList<MessageDto>>;
