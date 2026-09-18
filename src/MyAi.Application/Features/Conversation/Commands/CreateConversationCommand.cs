using MediatR;
using MyAi.Application.Features.Conversation;

namespace MyAi.Application.Features.Conversation.Commands;

public record CreateConversationCommand(string? Title = null) : IRequest<ConversationDto>;
