using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAi.Api.Contracts;
using MyAi.Application.Features.CustomAi;
using MyAi.Application.Features.CustomAi.Commands;
using MyAi.Application.Features.CustomAi.Queries;
using MyAi.Application.Features.Settings;
using MyAi.Application.Features.Settings.Commands;
using MyAi.Application.Features.Settings.Queries;

namespace MyAi.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Current user's full avatar/voice/animation settings.</summary>
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettingsAsync(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetUserSettingsQuery(), ct));

    /// <summary>Partial settings update. Locked (subscriber-only) fields → 403.</summary>
    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettingsAsync(
        [FromBody] UpdateSettingsRequest request, CancellationToken ct)
    {
        var command = new UpdateUserSettingsCommand(
            request.PreferredLanguage,
            request.VoiceName,
            request.VoiceSpeed,
            request.VoicePitch,
            request.DefaultExpression,
            request.ThemePreference,
            request.ShowSubtitles,
            request.AutoPlayAudio,
            request.EnabledAnimations,
            request.BlinkEnabled,
            request.BlinkFrequency,
            request.ThinkingPoseEnabled,
            request.AvatarModelId,
            request.ClearAvatarModel);

        return Ok(await _mediator.Send(command, ct));
    }

    /// <summary>Feature flags for the caller's role (drives UI lock/unlock).</summary>
    [HttpGet("settings/feature-flags")]
    public async Task<IActionResult> GetFeatureFlagsAsync(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetMyFeatureFlagsQuery(), ct));

    /// <summary>Get the caller's custom AI config (masked key only).</summary>
    [HttpGet("custom-ai")]
    public async Task<IActionResult> GetCustomAiConfigAsync(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetCustomAiConfigQuery(), ct));

    /// <summary>Save (upsert) the caller's custom AI API key + preferred model.</summary>
    [HttpPost("custom-ai")]
    public async Task<IActionResult> SaveCustomAiConfigAsync(
        [FromBody] CustomAiConfigRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(new SaveCustomAiConfigCommand(request.ApiKey, request.PreferredModel), ct));

    /// <summary>Remove the custom AI config and return to the system default provider.</summary>
    [HttpDelete("custom-ai")]
    public async Task<IActionResult> RemoveCustomAiConfigAsync(CancellationToken ct) =>
        Ok(new { removed = await _mediator.Send(new RemoveCustomAiConfigCommand(), ct) });

    /// <summary>Test the stored custom AI key against OpenRouter.</summary>
    [HttpPost("custom-ai/test")]
    public async Task<IActionResult> TestCustomAiKeyAsync(CancellationToken ct) =>
        Ok(await _mediator.Send(new TestCustomAiKeyQuery(), ct));
}
