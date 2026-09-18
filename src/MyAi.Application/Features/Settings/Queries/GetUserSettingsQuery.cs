using MediatR;
using MyAi.Application.Features.Settings;

namespace MyAi.Application.Features.Settings.Queries;

public record GetUserSettingsQuery : IRequest<UserSettingsDto>;
