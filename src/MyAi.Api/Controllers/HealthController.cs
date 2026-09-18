using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAi.Application.Common.Interfaces;
using MyAi.Infrastructure.Persistence;

namespace MyAi.Api.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    private readonly ICacheService _cacheService;

    public HealthController(ApplicationDbContext dbContext, ICacheService cacheService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
    }

    /// <summary>Liveness: process is up. 200 always.</summary>
    [HttpGet("/health")]
    [AllowAnonymous]
    public IActionResult GetLivenessAsync() => Ok(new { status = "healthy", timestamp = DateTime.UtcNow });

    /// <summary>Readiness: checks database + cache connectivity.</summary>
    [HttpGet("/health/ready")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReadinessAsync(CancellationToken ct)
    {
        var checks = new Dictionary<string, string>();

        var dbHealthy = false;

        try
        {
            dbHealthy = await _dbContext.Database.CanConnectAsync(ct);
            checks["database"] = dbHealthy ? "healthy" : "unreachable";
        }
        catch (Exception ex)
        {
            checks["database"] = $"error: {ex.Message}";
        }

        var cacheHealthy = false;

        try
        {
            await _cacheService.SetAsync("health:probe", new HealthProbe(true), TimeSpan.FromSeconds(5), ct);
            cacheHealthy = await _cacheService.GetAsync<HealthProbe>("health:probe", ct) is not null;
            checks["cache"] = cacheHealthy ? "healthy" : "degraded";
        }
        catch (Exception ex)
        {
            checks["cache"] = $"error: {ex.Message}";
        }

        var overall = dbHealthy ? "healthy" : "unhealthy";

        return StatusCode(
            dbHealthy ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable,
            new { status = overall, checks, timestamp = DateTime.UtcNow });
    }

    private sealed record HealthProbe(bool Ok);
}
