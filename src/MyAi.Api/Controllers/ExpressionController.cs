using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAi.Application.Features.Avatar;
using MyAi.Application.Features.Avatar.Queries;

namespace MyAi.Api.Controllers;

[ApiController]
[Route("api/v1/expressions")]
[Authorize]
public class ExpressionController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExpressionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Expressions accessible to the caller's role (public: 4, subscriber+: 12).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAccessibleExpressionsAsync(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetAccessibleExpressionsQuery(), ct));
}
