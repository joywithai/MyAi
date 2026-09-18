using MediatR;
using MyAi.Application.Features.Auth;

namespace MyAi.Application.Features.Auth.Commands;

public record RegisterUserCommand(
    string Email,
    string Password,
    string DisplayName,
    string? IpAddress = null) : IRequest<TokenResponse>;
