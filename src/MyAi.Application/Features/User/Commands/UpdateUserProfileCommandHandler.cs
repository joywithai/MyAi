using AutoMapper;
using MediatR;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.User;

namespace MyAi.Application.Features.User.Commands;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UserProfileDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public UpdateUserProfileCommandHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<UserProfileDto> Handle(UpdateUserProfileCommand command, CancellationToken ct)
    {
        var userId = _currentUserService.GetUserId();
        var user = await _userRepository.GetByIdAsync(userId, ct)
                   ?? throw new Common.Exceptions.NotFoundException("User", userId);

        user.UpdateProfile(command.DisplayName);
        await _userRepository.UpdateAsync(user, ct);

        return _mapper.Map<UserProfileDto>(user);
    }
}
