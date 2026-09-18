using FluentValidation;

namespace MyAi.Application.Features.Chat.Validators;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    private static readonly string[] ValidLanguages = { "bn", "en" };

    public SendMessageCommandValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MaximumLength(500).WithMessage("Message cannot exceed 500 characters.");

        RuleFor(x => x.Language)
            .Must(l => ValidLanguages.Contains(l?.ToLowerInvariant() ?? string.Empty))
            .WithMessage("Language must be 'bn' or 'en'.");

        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("ConversationId is required.");
    }
}
