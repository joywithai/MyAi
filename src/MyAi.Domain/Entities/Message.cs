using System.Text.Json;
using MyAi.Domain.Enums;
using MyAi.Domain.ValueObjects;

namespace MyAi.Domain.Entities;

public class Message : Entity
{
    private Message() { } // EF Core

    private Message(
        Guid conversationId,
        MessageRole role,
        string content,
        string? script,
        Language language,
        List<ExpressionSegment>? expressionSegments,
        AudioMetadata? audio)
    {
        ConversationId = conversationId;
        Role = role;
        Content = content;
        Script = script;
        Language = language;
        ExpressionSegments = expressionSegments;
        CreatedAt = DateTime.UtcNow;

        if (audio is not null)
        {
            AudioUrl = audio.AudioUrl;
            AudioDurationMs = audio.DurationMs;
            AudioContentType = audio.ContentType;
            WordBoundaries = audio.WordBoundaries.ToList();
        }
    }

    public Guid ConversationId { get; private set; }

    public MessageRole Role { get; private set; }

    public string Content { get; private set; }

    public string? Script { get; private set; }

    public Language Language { get; private set; }

    public List<ExpressionSegment>? ExpressionSegments { get; private set; }

    public string? AudioUrl { get; private set; }

    public int? AudioDurationMs { get; private set; }

    public string? AudioContentType { get; private set; }

    public List<WordBoundary>? WordBoundaries { get; private set; }

    public int? TokensUsed { get; private set; }

    public string? AiModelUsed { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Message CreateUserMessage(Guid conversationId, string content, Language language)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content is required.");
        }

        return new Message(conversationId, MessageRole.User, content.Trim(), null, language, null, null);
    }

    public static Message CreateAssistantMessage(
        Guid conversationId,
        string content,
        string? script,
        Language language,
        List<ExpressionSegment>? segments,
        AudioMetadata? audio)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Assistant message content is required.");
        }

        return new Message(conversationId, MessageRole.Assistant, content.Trim(), script, language, segments, audio)
        {
            TokensUsed = null,
            AiModelUsed = null
        };
    }

    public void AttachAudio(AudioMetadata audio)
    {
        AudioUrl = audio.AudioUrl;
        AudioDurationMs = audio.DurationMs;
        AudioContentType = audio.ContentType;
        WordBoundaries = audio.WordBoundaries.ToList();
    }

    public void AttachAiModel(string? model, int? tokensUsed)
    {
        AiModelUsed = model;
        TokensUsed = tokensUsed;
    }

    public static readonly JsonSerializerOptions SegmentSerializerOptions = new(JsonSerializerDefaults.Web);
}
