using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Auth;
using MyAi.Domain.Entities;

namespace MyAi.Application.Features.Auth.Commands;

public class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, TokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly IAvatarModelRepository _avatarModelRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        ISettingsRepository settingsRepository,
        IAvatarModelRepository avatarModelRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator)
    {
        _userRepository = userRepository;
        _settingsRepository = settingsRepository;
        _avatarModelRepository = avatarModelRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task<TokenResponse> Handle(RegisterUserCommand command, CancellationToken ct)
    {
        var email = command.Email.Trim().ToLowerInvariant();

        if (await _userRepository.ExistsByEmailAsync(email, ct))
        {
            throw new ConflictException(ErrorMessages.EmailAlreadyExists);
        }

        var user = User.Create(email, _passwordHasher.Hash(command.Password), command.DisplayName);
        await _userRepository.AddAsync(user, ct);

        var settings = UserSettings.CreateDefault(user.Id);
        await AssignDefaultAvatar(settings, ct);
        await _settingsRepository.AddAsync(settings, ct);

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

    private async Task AssignDefaultAvatar(UserSettings settings, CancellationToken ct)
    {
        var defaultModel = await _avatarModelRepository.GetDefaultAsync(ct);
        settings.SelectAvatarModel(defaultModel?.Id);
    }
}
