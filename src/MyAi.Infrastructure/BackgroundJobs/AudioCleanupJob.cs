using MyAi.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using MyAi.Application.Common.Interfaces.Repositories;

namespace MyAi.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire recurring job: deletes TTS audio older than 30 days
/// (storage file + clears message.audio_url). Runs daily.
/// </summary>
public class AudioCleanupJob
{
    private readonly IMessageRepository _messageRepository;

    private readonly IStorageService _storageService;

    private readonly ILogger<AudioCleanupJob> _logger;

    public AudioCleanupJob(
        IMessageRepository messageRepository,
        IStorageService storageService,
        ILogger<AudioCleanupJob> logger)
    {
        _messageRepository = messageRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("AudioCleanupJob started");

        // Messages whose audio_url points to storage older than 30 days get cleared.
        var cleared = await _messageRepository.CleanupOldAudioAsync(olderThanDays: 30, ct);

        _logger.LogInformation("AudioCleanupJob finished: {Count} audio references cleared", cleared);
    }
}
