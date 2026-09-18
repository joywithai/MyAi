using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAi.Api.Contracts;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Models;
using MyAi.Application.Features.Admin;
using MyAi.Application.Features.Admin.Commands;
using MyAi.Application.Features.Admin.Queries;
using MyAi.Application.Features.Avatar;
using MyAi.Application.Features.Avatar.Commands;
using MyAi.Application.Features.Avatar.Queries;

namespace MyAi.Api.Controllers;

/// <summary>Admin-only management surface: users, flags, catalog, system settings, audit logs.</summary>
[ApiController]
[Route("api/v1/admin")]
[Authorize(Policy = PolicyConstants.AdminOnly)]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ── Users ──────────────────────────────────────────────────────────────────

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsersAsync(
        [FromQuery] PagedRequest request, [FromQuery] string? role, [FromQuery] string? status, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetAllUsersQuery(request, role, status), ct));

    [HttpPut("users/{userId:guid}/role")]
    public async Task<IActionResult> UpdateUserRoleAsync(
        Guid userId, [FromBody] UpdateUserRoleRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(new UpdateUserRoleCommand(userId, request.Role), ct));

    [HttpPut("users/{userId:guid}/status")]
    public async Task<IActionResult> UpdateUserStatusAsync(
        Guid userId, [FromBody] UpdateUserStatusRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(new UpdateUserStatusCommand(userId, request.Status), ct));

    // ── Feature flags ──────────────────────────────────────────────────────────

    [HttpGet("feature-flags")]
    public async Task<IActionResult> GetFeatureFlagsAsync(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetRoleFeatureFlagsQuery(), ct));

    [HttpPut("feature-flags/{role}")]
    public async Task<IActionResult> UpdateFeatureFlagsAsync(
        string role, [FromBody] UpdateRoleFeatureFlagsRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(new UpdateRoleFeatureFlagsCommand(
            role,
            request.CanUseCustomApiKey,
            request.CanAccessAllExpressions,
            request.CanAccessAllAnimations,
            request.CanSelectAvatarModel,
            request.CanCustomizeVoice,
            request.CanAccessChatHistory,
            request.MaxConversationHistory,
            request.MaxMessagesPerDay), ct));

    // ── Avatar models ──────────────────────────────────────────────────────────

    [HttpGet("avatar-models")]
    public async Task<IActionResult> GetAvatarModelsAsync(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetAllAvatarModelsQuery(), ct));

    /// <summary>Save the GLOBAL avatar scene (camera/model/lights) — applies to all users.</summary>
    [HttpPut("avatar-scene-config")]
    public async Task<IActionResult> UpdateAvatarSceneConfigAsync(
        [FromBody] Application.Features.Avatar.AvatarSceneConfigDto config, CancellationToken ct) =>
        Ok(await _mediator.Send(new UpdateAvatarSceneConfigCommand(config), ct));

    [HttpPost("avatar-models")]
    public async Task<IActionResult> CreateAvatarModelAsync(
        [FromBody] CreateAvatarModelRequest request, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await _mediator.Send(new CreateAvatarModelCommand(
            request.Name, request.FileUrl, request.ThumbnailUrl, request.MinRole,
            request.Description, request.IsDefault), ct));

    [HttpPut("avatar-models/{id:guid}")]
    public async Task<IActionResult> UpdateAvatarModelAsync(
        Guid id, [FromBody] UpdateAvatarModelRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(new UpdateAvatarModelCommand(
            id, request.Name, request.Description, request.ThumbnailUrl,
            request.MinRole, request.IsDefault, request.IsActive), ct));

    [HttpDelete("avatar-models/{id:guid}")]
    public async Task<IActionResult> DeactivateAvatarModelAsync(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateAvatarModelCommand(id), ct);
        return NoContent();
    }

    // ── Expressions ────────────────────────────────────────────────────────────

    [HttpGet("expressions")]
    public async Task<IActionResult> GetExpressionsAsync(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetAllExpressionsQuery(), ct));

    [HttpPost("expressions")]
    public async Task<IActionResult> CreateExpressionAsync(
        [FromBody] CreateExpressionRequest request, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await _mediator.Send(new CreateExpressionCommand(
            request.Name, request.DisplayName, request.Description, request.MinRole), ct));

    [HttpPut("expressions/{id:guid}")]
    public async Task<IActionResult> UpdateExpressionAsync(
        Guid id, [FromBody] UpdateExpressionRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(new UpdateExpressionCommand(
            id, request.DisplayName, request.Description, request.MinRole, request.IsActive), ct));

    // ── Animations ─────────────────────────────────────────────────────────────

    [HttpGet("animations")]
    public async Task<IActionResult> GetAnimationsAsync(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetAllAnimationsQuery(), ct));

    [HttpPost("animations")]
    public async Task<IActionResult> CreateAnimationAsync(
        [FromBody] CreateAnimationRequest request, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await _mediator.Send(new CreateAnimationCommand(
            request.Name, request.DisplayName, request.Description, request.MinRole), ct));

    [HttpPut("animations/{id:guid}")]
    public async Task<IActionResult> UpdateAnimationAsync(
        Guid id, [FromBody] UpdateAnimationRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(new UpdateAnimationCommand(
            id, request.DisplayName, request.Description, request.MinRole, request.IsActive), ct));

    // ── System settings ────────────────────────────────────────────────────────

    [HttpGet("system-settings")]
    public async Task<IActionResult> GetSystemSettingsAsync(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetAllSystemSettingsQuery(), ct));

    [HttpPut("system-settings/{key}")]
    public async Task<IActionResult> UpdateSystemSettingAsync(
        string key, [FromBody] UpdateSystemSettingRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(new UpdateSystemSettingCommand(key, request.Value), ct));

    // ── Audit logs ─────────────────────────────────────────────────────────────

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogsAsync([FromQuery] PagedRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetAuditLogsQuery(request), ct));
}
