using MediatR;
using MyAi.Application.Features.CustomAi;

namespace MyAi.Application.Features.CustomAi.Queries;

public record GetCustomAiConfigQuery : IRequest<CustomAiConfigDto?>;
