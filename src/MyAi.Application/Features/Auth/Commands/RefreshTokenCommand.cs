using MediatR;
using MyAi.Application.Features.Auth;

namespace MyAi.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken, string? IpAddress = null) : IRequest<TokenResponse>;
