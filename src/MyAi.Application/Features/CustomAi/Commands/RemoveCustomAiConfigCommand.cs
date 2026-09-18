using MediatR;

namespace MyAi.Application.Features.CustomAi.Commands;

public record RemoveCustomAiConfigCommand : IRequest<bool>;
