namespace MyAi.Application.Features.Conversation;

public record ConversationDto(
    Guid Id,
    Guid UserId,
    string? Title,
    int MessageCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
