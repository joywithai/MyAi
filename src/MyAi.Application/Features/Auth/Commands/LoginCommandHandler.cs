using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Auth;

namespace MyAi.Application.Features.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, TokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task<TokenResponse> Handle(LoginCommand command, CancellationToken ct)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.FindByEmailAsync(email, ct);

        if (user is null || !_passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            throw new UnauthorizedException(ErrorMessages.InvalidCredentials);
        }

        if (!user.CanLogin())
        {
            throw new UnauthorizedException(
                user.Status == Domain.Enums.UserStatus.Banned
                    ? ErrorMessages.AccountBanned
                    : ErrorMessages.AccountInactive);
        }

        user.SetLastLogin(DateTime.UtcNow);
        await _userRepository.UpdateAsync(user, ct);

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = await _refreshTokenGenerator.GenerateAsync(user.Id, command.IpAddress, ct);

        return new TokenResponse(
            accessToken,
            refreshToken.Token,
            _jwtTokenGenerator.GetAccessTokenExpirySeconds(),
            user.Id,
            user.Email,
            user.DisplayName,
            RoleConstants.ToRoleString(user.Role));
    }
}
