using MyAi.Application.Common.Models;

namespace MyAi.Application.Features.Chat;

public record ExpressionSegmentDto(string Expression, string Text);

public record WordBoundaryDto(string Word, int StartTimeMs, int DurationMs, int TextOffset, int WordLength);

public record MessageDto(
    Guid Id,
    Guid ConversationId,
    string Role,
    string Content,
    string Language,
    List<ExpressionSegmentDto>? ExpressionSegments,
    string? AudioUrl,
    int? AudioDurationMs,
    string? AudioContentType,
    List<WordBoundaryDto>? WordBoundaries,
    string? AiModelUsed,
    DateTime CreatedAt);

public record ChatResponse(
    Guid MessageId,
    Guid ConversationId,
    string Reply,
    string? Script,
    string Language,
    List<ExpressionSegmentDto> Segments,
    string? AudioBase64,
    string? ContentType,
    int? AudioDurationMs,
    List<WordBoundaryDto> WordBoundaries,
    string? AiModelUsed);
