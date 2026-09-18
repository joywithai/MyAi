using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAi.Api.Contracts;
using MyAi.Application.Features.Auth;
using MyAi.Application.Features.Auth.Commands;
using MyAi.Application.Common.Interfaces;

namespace MyAi.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    private readonly ICurrentUserService _currentUserService;

    public AuthController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>Register a new user. 201 with tokens; 409 on duplicate email.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var command = new RegisterUserCommand(
            request.Email, request.Password, request.DisplayName, _currentUserService.GetIpAddress());

        var result = await _mediator.Send(command, ct);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Login with email + password. 200 with tokens; 401 on bad credentials.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new LoginCommand(request.Email, request.Password, _currentUserService.GetIpAddress()), ct);

        return Ok(result);
    }

    /// <summary>Rotates the refresh token and issues a new access token.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new RefreshTokenCommand(request.RefreshToken, _currentUserService.GetIpAddress()), ct);

        return Ok(result);
    }

    /// <summary>Revokes the given refresh token. 204.</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LogoutAsync([FromBody] LogoutRequest request, CancellationToken ct)
    {
        await _mediator.Send(new LogoutCommand(request.RefreshToken), ct);
        return NoContent();
    }
}
