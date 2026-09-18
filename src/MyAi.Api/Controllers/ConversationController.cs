using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Models;
using MyAi.Application.Features.Chat;
using MyAi.Application.Features.Chat.Queries;
using MyAi.Application.Features.Conversation;
using MyAi.Application.Features.Conversation.Commands;
using MyAi.Application.Features.Conversation.Queries;

namespace MyAi.Api.Controllers;

[ApiController]
[Route("api/v1/conversations")]
[Authorize]
[ServiceFilter(typeof(Filters.AuthorizeResourceFilter))]
public class ConversationController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConversationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Paginated list of the current user's conversations.</summary>
    [HttpGet]
    public async Task<IActionResult> GetConversationsAsync([FromQuery] PagedRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetUserConversationsQuery(request), ct));

    /// <summary>Paginated messages of one conversation (ownership enforced).</summary>
    [HttpGet("{conversationId:guid}/messages")]
    public async Task<IActionResult> GetConversationMessagesAsync(
        Guid conversationId, [FromQuery] PagedRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetConversationHistoryQuery(conversationId, request), ct));

    /// <summary>Create a new conversation.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateConversationAsync(CancellationToken ct)
    {
        var conversation = await _mediator.Send(new CreateConversationCommand(), ct);
        return StatusCode(StatusCodes.Status201Created, conversation);
    }

    /// <summary>Soft-delete a conversation (ownership enforced). 204.</summary>
    [HttpDelete("{conversationId:guid}")]
    public async Task<IActionResult> DeleteConversationAsync(Guid conversationId, CancellationToken ct)
    {
        await _mediator.Send(new DeleteConversationCommand(conversationId), ct);
        return NoContent();
    }
}
