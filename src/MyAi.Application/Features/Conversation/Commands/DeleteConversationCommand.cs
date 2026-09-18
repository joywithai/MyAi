using MediatR;

namespace MyAi.Application.Features.Conversation.Commands;

public record DeleteConversationCommand(Guid ConversationId) : IRequest<bool>;
