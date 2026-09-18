namespace MyAi.Infrastructure.AI.Shared;

/// <summary>
/// Maps system expression names to VRM 1.0 preset expression names.
/// The frontend's ExpressionController uses the same mapping (constants/expressions.ts).
/// </summary>
public static class ExpressionMapper
{
    public static readonly IReadOnlyDictionary<string, string> VrmMappings = new Dictionary<string, string>
    {
        ["NEUTRAL"] = "neutral",
        ["HAPPY"] = "happy",
        ["SAD"] = "sad",
        ["ANGRY"] = "angry",
        ["SURPRISED"] = "surprised",
        ["RELAXED"] = "relaxed",
        ["EXCITED"] = "happy",
        ["CONFUSED"] = "surprised",
        ["THOUGHTFUL"] = "neutral",
        ["CONCERNED"] = "sad",
        ["FRIENDLY"] = "happy",
        ["SERIOUS"] = "neutral"
    };

    public static string MapToVrmExpression(string expression) =>
        VrmMappings.TryGetValue(ExpressionValidator.Normalize(expression), out var vrmName)
            ? vrmName
            : "neutral";

    public static IReadOnlyDictionary<string, string> GetMappingDictionary() => VrmMappings;
}
