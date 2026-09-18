using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAi.Api.Contracts;
using MyAi.Application.Features.Avatar;
using MyAi.Application.Features.Avatar.Commands;
using MyAi.Application.Features.Avatar.Queries;
using MyAi.Application.Features.Settings;

namespace MyAi.Api.Controllers;

[ApiController]
[Route("api/v1/avatars")]
[Authorize]
public class AvatarController : ControllerBase
{
    private readonly IMediator _mediator;

    public AvatarController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Avatar models accessible to the caller's role.</summary>
    [HttpGet("models")]
    public async Task<IActionResult> GetAvailableModelsAsync(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetAvailableAvatarModelsQuery(), ct));

    /// <summary>Select the caller's avatar model (flag + role checked). 403/404 on failure.</summary>
    [HttpPut("select")]
    public async Task<IActionResult> SelectAvatarModelAsync(
        [FromBody] SelectAvatarRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(new SelectAvatarModelCommand(request.ModelId), ct));
}
