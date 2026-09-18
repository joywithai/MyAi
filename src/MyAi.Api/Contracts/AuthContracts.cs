using System.ComponentModel.DataAnnotations;

namespace MyAi.Api.Contracts;

public class RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    [Required, MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;
}
