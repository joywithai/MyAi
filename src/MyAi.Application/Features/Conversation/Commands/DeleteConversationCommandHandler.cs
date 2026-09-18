using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Common.Constants;

namespace MyAi.Application.Features.Conversation.Commands;

public class DeleteConversationCommandHandler : IRequestHandler<DeleteConversationCommand, bool>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUserService;

    public DeleteConversationCommandHandler(
        IConversationRepository conversationRepository,
        ICacheService cacheService,
        ICurrentUserService currentUserService)
    {
        _conversationRepository = conversationRepository;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteConversationCommand command, CancellationToken ct)
    {
        var userId = _currentUserService.GetUserId();
        var conversation = await _conversationRepository.GetByIdAsync(command.ConversationId, ct);

        if (conversation is null || conversation.IsDeleted)
        {
            throw new NotFoundException(ErrorMessages.ConversationNotFound);
        }

        if (conversation.UserId != userId)
        {
            throw new ForbiddenException(ErrorMessages.ConversationNotOwned);
        }

        conversation.SoftDelete();
        await _conversationRepository.UpdateAsync(conversation, ct);
        await _cacheService.RemoveAsync(CacheKeys.ConversationsPage(userId, 1), ct);

        return true;
    }
}
