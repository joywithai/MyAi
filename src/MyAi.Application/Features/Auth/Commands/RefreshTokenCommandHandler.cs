using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Auth;
using MyAi.Domain.Entities;

namespace MyAi.Application.Features.Auth.Commands;

/// <summary>Refresh token rotation: old token revoked (single-use), new pair issued.</summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task<TokenResponse> Handle(RefreshTokenCommand command, CancellationToken ct)
    {
        var existingToken = await _userRepository.FindRefreshTokenAsync(command.RefreshToken, ct);

        if (existingToken is null || !existingToken.IsActive(DateTime.UtcNow))
        {
            throw new UnauthorizedException(ErrorMessages.InvalidRefreshToken);
        }

        var user = await _userRepository.GetByIdAsync(existingToken.UserId, ct);

        if (user is null || !user.CanLogin())
        {
            throw new UnauthorizedException(ErrorMessages.InvalidRefreshToken);
        }

        var newRefreshToken = await _refreshTokenGenerator.GenerateAsync(user.Id, command.IpAddress, ct);
        existingToken.Revoke(newRefreshToken.Token);
        await _userRepository.UpdateAsync(user, ct);

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);

        return new TokenResponse(
            accessToken,
            newRefreshToken.Token,
            _jwtTokenGenerator.GetAccessTokenExpirySeconds(),
            user.Id,
            user.Email,
            user.DisplayName,
            RoleConstants.ToRoleString(user.Role));
    }
}
