namespace MyAi.Application.Features.CustomAi;

public record CustomAiConfigDto(
    string ProviderName,
    string? PreferredModel,
    bool HasKey,
    string? MaskedKey,
    bool IsActive,
    DateTime? LastVerifiedAt);

public record CustomAiKeyTestResultDto(bool IsValid, string? ErrorDetail, string? ModelInfo);
