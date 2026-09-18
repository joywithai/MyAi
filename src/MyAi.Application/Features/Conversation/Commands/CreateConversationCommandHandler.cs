using MediatR;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Conversation;
using MyAi.Domain.Entities;

namespace MyAi.Application.Features.Conversation.Commands;

public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, ConversationDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateConversationCommandHandler(
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService)
    {
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ConversationDto> Handle(CreateConversationCommand command, CancellationToken ct)
    {
        var userId = _currentUserService.GetUserId();
        var conversation = Conversation.Create(userId, command.Title);

        await _conversationRepository.AddAsync(conversation, ct);

        return new ConversationDto(
            conversation.Id,
            conversation.UserId,
            conversation.Title,
            conversation.MessageCount,
            conversation.CreatedAt,
            conversation.UpdatedAt);
    }
}
