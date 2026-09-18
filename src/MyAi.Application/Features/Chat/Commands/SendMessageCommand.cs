using MediatR;
using MyAi.Application.Features.Chat;

namespace MyAi.Application.Features.Chat.Commands;

public record SendMessageCommand(
    Guid ConversationId,
    string Message,
    string Language) : IRequest<ChatResponse>;
