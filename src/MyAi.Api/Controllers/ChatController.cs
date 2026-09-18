using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAi.Api.Contracts;
using MyAi.Application.Features.Chat;
using MyAi.Application.Features.Chat.Commands;

namespace MyAi.Api.Controllers;

[ApiController]
[Route("api/v1/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Ask the AI avatar. Returns text + expression segments + base64 audio + word boundaries.
    /// Rate limited per role (public 50/day, subscriber 500/day).
    /// </summary>
    [HttpPost("ask")]
    public async Task<IActionResult> AskAsync([FromBody] ChatRequest request, CancellationToken ct)
    {
        var conversationId = request.ConversationId;

        if (conversationId is null || conversationId == Guid.Empty)
        {
            var created = await _mediator.Send(
                new Application.Features.Conversation.Commands.CreateConversationCommand(), ct);
            conversationId = created.Id;
        }

        var response = await _mediator.Send(
            new SendMessageCommand(conversationId.Value, request.Message, request.Language), ct);

        return Ok(response);
    }

    /// <summary>Client-side cancellation handshake. 204.</summary>
    [HttpPost("stop")]
    public async Task<IActionResult> StopAsync(CancellationToken ct)
    {
        // Generation cancellation happens through the AbortSignal the client passes
        // to its fetch call; the server observes RequestAborted on AskAsync.
        await Task.CompletedTask;
        return NoContent();
    }
}
