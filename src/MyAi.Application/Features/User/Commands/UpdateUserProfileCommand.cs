using MediatR;
using MyAi.Application.Features.User;

namespace MyAi.Application.Features.User.Commands;

public record UpdateUserProfileCommand(string DisplayName) : IRequest<UserProfileDto>;
