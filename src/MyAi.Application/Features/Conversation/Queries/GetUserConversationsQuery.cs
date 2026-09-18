using MediatR;
using MyAi.Application.Common.Models;
using MyAi.Application.Features.Conversation;

namespace MyAi.Application.Features.Conversation.Queries;

public record GetUserConversationsQuery(PagedRequest Request) : IRequest<PaginatedList<ConversationDto>>;
