using MediatR;
using Microsoft.Extensions.Logging;
using MyAi.Application.Common.Ai;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Common.Tts;
using MyAi.Application.Features.Chat;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;
using MyAi.Domain.ValueObjects;

namespace MyAi.Application.Features.Chat.Commands;

/// <summary>
/// Facade that coordinates conversation validation, rate limiting, AI generation,
/// TTS synthesis, persistence and event publishing for a single chat turn.
/// Graceful degradation: AI down → ExternalServiceException (handled centrally);
/// TTS down → text-only response; storage down → inline audio only.
/// </summary>
public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, ChatResponse>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ICustomAiConfigRepository _customAiConfigRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly IFeatureFlagsRepository _featureFlagsRepository;
    private readonly IExpressionRepository _expressionRepository;
    private readonly IAiProviderFactory _aiProviderFactory;
    private readonly ITtsProviderFactory _ttsProviderFactory;
    private readonly IStorageService _storageService;
    private readonly IApiKeyEncryptionService _apiKeyEncryptionService;
    private readonly ISystemSettingRepository _systemSettingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SendMessageCommandHandler> _logger;

    public SendMessageCommandHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        ICustomAiConfigRepository customAiConfigRepository,
        ISettingsRepository settingsRepository,
        IFeatureFlagsRepository featureFlagsRepository,
        IExpressionRepository expressionRepository,
        IAiProviderFactory aiProviderFactory,
        ITtsProviderFactory ttsProviderFactory,
        IStorageService storageService,
        IApiKeyEncryptionService apiKeyEncryptionService,
        ISystemSettingRepository systemSettingRepository,
        ICurrentUserService currentUserService,
        ILogger<SendMessageCommandHandler> logger)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _customAiConfigRepository = customAiConfigRepository;
        _settingsRepository = settingsRepository;
        _featureFlagsRepository = featureFlagsRepository;
        _expressionRepository = expressionRepository;
        _aiProviderFactory = aiProviderFactory;
        _ttsProviderFactory = ttsProviderFactory;
        _storageService = storageService;
        _apiKeyEncryptionService = apiKeyEncryptionService;
        _systemSettingRepository = systemSettingRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ChatResponse> Handle(SendMessageCommand command, CancellationToken ct)
    {
        var userId = _currentUserService.GetUserId();
        var role = RoleConstants.ToUserRole(_currentUserService.GetUserRole());
        var language = ParseLanguage(command.Language);

        await ValidateConversationOwnership(command.ConversationId, userId, ct);
        await CheckDailyLimit(userId, role, ct);

        var conversation = (await _conversationRepository.GetByIdAsync(command.ConversationId, ct))!;

        // 1. Persist the user's message.
        var userMessage = Message.CreateUserMessage(command.ConversationId, command.Message, language);
        await _messageRepository.AddAsync(userMessage, ct);
        conversation.AddMessage(userMessage);
        if (conversation.Title is null)
        {
            conversation.UpdateTitle(TruncateTitle(command.Message));
        }
        await _conversationRepository.UpdateAsync(conversation, ct);

        // 2. Resolve AI provider (custom key > system default), settings and accessible expressions.
        var customConfig = await _customAiConfigRepository.GetByUserIdAsync(userId, ct);
        var settings = await _settingsRepository.GetByUserIdAsync(userId, ct)
                       ?? UserSettings.CreateDefault(userId);
        var providerName = await _systemSettingRepository.GetValueAsync("default_ai_provider", ct) ?? "openrouter";
        var defaultModel = await _systemSettingRepository.GetValueAsync("default_ai_model", ct);

        var decryptedKey = customConfig is { IsActive: true }
            ? TryDecrypt(customConfig.EncryptedApiKey)
            : null;

        var accessibleExpressions = (await _expressionRepository.GetAccessibleByRoleAsync(role, ct))
            .Select(e => e.Name)
            .ToList();

        var recentHistory = await _messageRepository.GetRecentByConversationAsync(command.ConversationId, 10, ct);
        var history = recentHistory
            .Where(m => m.Id != userMessage.Id)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatHistoryItem(
                m.Role == MessageRole.User ? "user" : "assistant",
                m.Content))
            .ToList();

        // 3. Ask the AI.
        var provider = _aiProviderFactory.Create(providerName, decryptedKey);
        var aiRequest = new AiRequest
        {
            Message = command.Message,
            Language = language,
            AccessibleExpressions = accessibleExpressions,
            History = history,
            PreferredModel = customConfig is { IsActive: true } ? customConfig.PreferredModel : defaultModel,
            ApiKey = decryptedKey
        };

        AiResult aiResult;
        try
        {
            aiResult = await provider.ProcessAsync(aiRequest, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "AI provider '{Provider}' failed for user {UserId}", provider.Name, userId);
            throw new ExternalServiceException("ai", ErrorMessages.AiUnavailable);
        }

        // 4. Synthesize speech for the reply script.
        var ttsText = aiResult.Segments.Count > 0
            ? string.Join(" ", aiResult.Segments.Select(s => s.Text))
            : aiResult.ReplyText;

        var voice = new VoiceSettings(language, settings.VoiceName, settings.VoiceSpeed, settings.VoicePitch);

        TtsResult ttsResult;
        try
        {
            var ttsProvider = _ttsProviderFactory.Create("edge");
            ttsResult = await ttsProvider.SynthesizeAsync(new TtsRequest
            {
                Text = ttsText,
                Language = language,
                VoiceSettings = voice
            }, ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "TTS synthesis failed for user {UserId}; returning text-only response", userId);
            ttsResult = new TtsResult
            {
                Audio = Array.Empty<byte>(),
                ContentType = string.Empty,
                DurationMs = 0,
                WordBoundaries = new List<WordBoundary>(),
                VoiceUsed = voice.VoiceName
            };
        }

        // 5. Store the audio when synthesis produced one (storage down → inline only).
        string? audioUrl = null;

        if (!ttsResult.IsEmpty)
        {
            try
            {
                var storagePath = $"audio/{userId}/{Guid.NewGuid():N}.mp3";
                audioUrl = await _storageService.UploadAsync(
                    new BlobUploadRequest(storagePath, ttsResult.Audio, ttsResult.ContentType), ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Audio storage upload failed; audio returned inline only");
            }
        }

        // 6. Persist the assistant message (fires MessageSentEvent via entity).
        var assistantMessage = Message.CreateAssistantMessage(
            command.ConversationId,
            aiResult.ReplyText,
            aiResult.Script,
            language,
            aiResult.Segments,
            ttsResult.IsEmpty
                ? null
                : AudioMetadata.Create(
                    audioUrl ?? string.Empty, ttsResult.DurationMs, ttsResult.ContentType, ttsResult.WordBoundaries));

        assistantMessage.AttachAiModel(aiResult.ModelUsed, aiResult.TokensUsed);
        await _messageRepository.AddAsync(assistantMessage, ct);
        conversation.AddMessage(assistantMessage);
        await _conversationRepository.UpdateAsync(conversation, ct);

        return BuildChatResponse(assistantMessage, aiResult, ttsResult);
    }

    private async Task ValidateConversationOwnership(Guid conversationId, Guid userId, CancellationToken ct)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, ct);

        if (conversation is null || conversation.IsDeleted)
        {
            throw new NotFoundException(ErrorMessages.ConversationNotFound);
        }

        if (conversation.UserId != userId)
        {
            throw new ForbiddenException(ErrorMessages.ConversationNotOwned);
        }
    }

    private async Task CheckDailyLimit(Guid userId, UserRole role, CancellationToken ct)
    {
        var flags = await _featureFlagsRepository.GetByRoleAsync(role, ct);

        if (flags is null || flags.MaxMessagesPerDay < 0)
        {
            return; // unlimited or unknown role
        }

        var usedToday = await _messageRepository.GetDailyCountAsync(userId, ct);

        if (usedToday >= flags.MaxMessagesPerDay)
        {
            throw new RateLimitExceededException(ErrorMessages.DailyLimitReached, SecondsUntilMidnight());
        }
    }

    private string? TryDecrypt(string encrypted)
    {
        try
        {
            return _apiKeyEncryptionService.Decrypt(encrypted);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to decrypt custom AI key; falling back to system provider");
            return null;
        }
    }

    private static string TruncateTitle(string message) =>
        message.Length <= 60 ? message : message[..60];

    private static Language ParseLanguage(string language) =>
        string.Equals(language, "en", StringComparison.OrdinalIgnoreCase) ? Language.En : Language.Bn;

    private static int SecondsUntilMidnight()
    {
        var now = DateTime.UtcNow;
        return Math.Max(1, (int)(now.Date.AddDays(1) - now).TotalSeconds);
    }

    private static ChatResponse BuildChatResponse(Message message, AiResult aiResult, TtsResult ttsResult)
    {
        return new ChatResponse(
            message.Id,
            message.ConversationId,
            aiResult.ReplyText,
            aiResult.Script,
            aiResult.Language,
            aiResult.Segments
                .Select(s => new ExpressionSegmentDto(s.Expression, s.Text))
                .ToList(),
            ttsResult.IsEmpty ? null : Convert.ToBase64String(ttsResult.Audio),
            ttsResult.IsEmpty ? null : ttsResult.ContentType,
            ttsResult.IsEmpty ? null : ttsResult.DurationMs,
            ttsResult.WordBoundaries
                .Select(w => new WordBoundaryDto(w.Word, w.StartTimeMs, w.DurationMs, w.TextOffset, w.WordLength))
                .ToList(),
            aiResult.ModelUsed);
    }
}
