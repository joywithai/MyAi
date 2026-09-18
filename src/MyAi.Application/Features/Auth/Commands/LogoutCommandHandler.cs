using MediatR;
using MyAi.Application.Common.Interfaces.Repositories;

namespace MyAi.Application.Features.Auth.Commands;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IUserRepository _userRepository;

    public LogoutCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(LogoutCommand command, CancellationToken ct)
    {
        var token = await _userRepository.FindRefreshTokenAsync(command.RefreshToken, ct);

        if (token is null)
        {
            return false;
        }

        token.Revoke(null);
        return true;
    }
}
