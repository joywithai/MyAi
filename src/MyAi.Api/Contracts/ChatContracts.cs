using System.ComponentModel.DataAnnotations;

namespace MyAi.Api.Contracts;

public class ChatRequest
{
    [Required, MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    [Required, RegularExpression("^(bn|en)$")]
    public string Language { get; set; } = "bn";

    public Guid? ConversationId { get; set; }
}

public class SelectAvatarRequest
{
    [Required]
    public Guid ModelId { get; set; }
}
