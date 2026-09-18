using MediatR;
using MyAi.Application.Features.Settings;

namespace MyAi.Application.Features.Avatar.Commands;

public record SelectAvatarModelCommand(Guid ModelId) : IRequest<UserSettingsDto>;
