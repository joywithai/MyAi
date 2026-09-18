using MediatR;
using MyAi.Application.Features.CustomAi;

namespace MyAi.Application.Features.CustomAi.Commands;

public record SaveCustomAiConfigCommand(string ApiKey, string? PreferredModel) : IRequest<CustomAiConfigDto>;
