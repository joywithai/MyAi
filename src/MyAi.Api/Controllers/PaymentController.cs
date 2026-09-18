using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAi.Api.Contracts;
using MyAi.Application.Features.Payment;
using MyAi.Application.Features.Payment.Commands;

namespace MyAi.Api.Controllers;

[ApiController]
[Route("api/v1/payments")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Start a payment session for a plan (demo → instant checkout data).</summary>
    [HttpPost("initiate")]
    public async Task<IActionResult> InitiatePaymentAsync(
        [FromBody] InitiatePaymentRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(new InitiatePaymentCommand(request.PlanId), ct));

    /// <summary>Process the payment result: success creates the subscription + upgrades role.</summary>
    [HttpPost("process")]
    public async Task<IActionResult> ProcessPaymentAsync(
        [FromBody] ProcessPaymentRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(
            new ProcessPaymentCommand(request.TransactionId, request.ProviderTransactionId, request.ProviderData),
            ct));

    /// <summary>Provider webhook receiver (signature verification in real mode).</summary>
    [HttpPost("webhook/{provider}")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleWebhookAsync(string provider, CancellationToken ct)
    {
        // Real-mode webhook handling (Stripe signature check / SSLCommerz IPN validation)
        // is part of the future integration; the route + dispatch structure is ready.
        await Task.CompletedTask;
        return Accepted(new { provider, received = true });
    }
}
