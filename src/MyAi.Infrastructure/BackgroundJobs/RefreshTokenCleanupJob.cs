using MyAi.Application.Common.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace MyAi.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire recurring job (weekly): batch-deletes expired and revoked refresh tokens.
/// </summary>
public class RefreshTokenCleanupJob
{
    private readonly IUserRepository _userRepository;

    private readonly ILogger<RefreshTokenCleanupJob> _logger;

    public RefreshTokenCleanupJob(IUserRepository userRepository, ILogger<RefreshTokenCleanupJob> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("RefreshTokenCleanupJob started");

        var deleted = await _userRepository.CleanupExpiredRefreshTokensAsync(ct);

        _logger.LogInformation("RefreshTokenCleanupJob finished: {Count} tokens removed", deleted);
    }
}
