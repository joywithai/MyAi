using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAi.Application.Features.Avatar;
using MyAi.Application.Features.Avatar.Queries;

namespace MyAi.Api.Controllers;

[ApiController]
[Route("api/v1/animations")]
[Authorize]
public class AnimationController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnimationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Animations accessible to the caller's role (public: breathing, subscriber+: all).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAccessibleAnimationsAsync(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetAccessibleAnimationsQuery(), ct));
}
