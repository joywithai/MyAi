namespace MyAi.Application.Features.Auth;

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    Guid UserId,
    string Email,
    string DisplayName,
    string Role);
