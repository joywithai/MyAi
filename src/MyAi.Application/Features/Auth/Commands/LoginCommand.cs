using MediatR;
using MyAi.Application.Features.Auth;

namespace MyAi.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password, string? IpAddress = null) : IRequest<TokenResponse>;
