using System.Text;
using MyAi.Domain.Enums;
using MyAi.Infrastructure.AI.Shared;

namespace MyAi.Infrastructure.AI.OpenRouter;

/// <summary>Builder pattern: constructs the system + user prompt for OpenRouter.</summary>
public class OpenRouterPromptBuilder
{
    public string BuildSystemPrompt(Language language, IReadOnlyList<string> accessibleExpressions)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are MyAi, a friendly 3D virtual avatar companion.");
        sb.AppendLine("You chat naturally with the user through speech and facial expressions.");
        sb.AppendLine();
        sb.AppendLine(BuildLanguageRules(language));
        sb.AppendLine();
        sb.AppendLine(BuildExpressionRules(accessibleExpressions));
        sb.AppendLine();
        sb.AppendLine(BuildResponseSchema());

        return sb.ToString();
    }

    public string BuildUserPrompt(string message, Language language) =>
        language == Language.Bn
            ? $"ব্যবহারকারীর বার্তা: {message}"
            : $"User message: {message}";

    public string BuildExpressionRules(IReadOnlyList<string> expressions)
    {
        var sb = new StringBuilder();
        var usable = expressions.Count > 0 ? expressions : ExpressionValidator.GetSupportedExpressions();

        sb.AppendLine("Facial expression rules:");
        sb.AppendLine($"- You may ONLY use these expressions: {string.Join(", ", usable)}.");
        sb.AppendLine("- Split your reply into 1-4 short segments when the mood changes.");
        sb.AppendLine("- Give each segment an expression from the allowed list.");
        sb.AppendLine("- Reply with a JSON object only, no markdown fences, e.g.:");
        sb.AppendLine("  {\"script\": [{\"expression\": \"HAPPY\", \"text\": \"...\"}], \"reply\": \"...\"}");
        sb.AppendLine("- The \"reply\" field must contain the full plain-text reply (all segments joined).");

        return sb.ToString();
    }

    public string BuildLanguageRules(Language language)
    {
        return language == Language.Bn
            ? "Language rules: Always reply in natural, conversational Bangla (বাংলা). Keep replies short (1-3 sentences)."
            : "Language rules: Always reply in natural, conversational English. Keep replies short (1-3 sentences).";
    }

    public string BuildResponseSchema() =>
        "Response format: {\"reply\": string, \"script\": [{\"expression\": string, \"text\": string}]}";
}
