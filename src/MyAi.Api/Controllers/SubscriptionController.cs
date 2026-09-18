using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAi.Application.Features.Subscription;
using MyAi.Application.Features.Subscription.Queries;

namespace MyAi.Api.Controllers;

[ApiController]
[Route("api/v1/subscriptions")]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubscriptionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>All active subscription plans.</summary>
    [HttpGet("plans")]
    public async Task<IActionResult> GetPlansAsync(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetSubscriptionPlansQuery(), ct));

    /// <summary>The caller's current active subscription (null when free user).</summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentSubscriptionAsync(CancellationToken ct)
    {
        var subscription = await _mediator.Send(new GetUserSubscriptionQuery(), ct);
        return subscription is null ? Ok(null) : Ok(subscription);
    }
}
