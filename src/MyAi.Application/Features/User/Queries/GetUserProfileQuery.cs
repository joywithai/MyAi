using MediatR;
using MyAi.Application.Features.User;

namespace MyAi.Application.Features.User.Queries;

public record GetUserProfileQuery : IRequest<UserProfileDto>;
