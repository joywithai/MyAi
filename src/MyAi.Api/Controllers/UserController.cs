using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAi.Api.Contracts;
using MyAi.Application.Features.User;
using MyAi.Application.Features.User.Commands;
using MyAi.Application.Features.User.Queries;

namespace MyAi.Api.Controllers;

[ApiController]
[Route("api/v1/user")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Current user's profile (includes active subscription).</summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfileAsync(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetUserProfileQuery(), ct));

    /// <summary>Update the display name.</summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfileAsync(
        [FromBody] UpdateProfileRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(new UpdateUserProfileCommand(request.DisplayName), ct));
}
