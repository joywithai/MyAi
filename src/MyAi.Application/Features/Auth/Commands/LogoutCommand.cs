using MediatR;

namespace MyAi.Application.Features.Auth.Commands;

public record LogoutCommand(string RefreshToken) : IRequest<bool>;
