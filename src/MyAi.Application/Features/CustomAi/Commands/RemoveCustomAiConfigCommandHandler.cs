using MediatR;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;

namespace MyAi.Application.Features.CustomAi.Commands;

public class RemoveCustomAiConfigCommandHandler : IRequestHandler<RemoveCustomAiConfigCommand, bool>
{
    private readonly ICustomAiConfigRepository _customAiConfigRepository;
    private readonly ICurrentUserService _currentUserService;

    public RemoveCustomAiConfigCommandHandler(
        ICustomAiConfigRepository customAiConfigRepository,
        ICurrentUserService currentUserService)
    {
        _customAiConfigRepository = customAiConfigRepository;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(RemoveCustomAiConfigCommand command, CancellationToken ct)
    {
        var userId = _currentUserService.GetUserId();
        var existing = await _customAiConfigRepository.GetByUserIdAsync(userId, ct);

        if (existing is null)
        {
            return false;
        }

        await _customAiConfigRepository.DeleteByUserIdAsync(userId, ct);
        return true;
    }
}
