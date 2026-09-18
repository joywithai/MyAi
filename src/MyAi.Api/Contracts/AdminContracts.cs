using System.ComponentModel.DataAnnotations;

namespace MyAi.Api.Contracts;

public class UpdateUserRoleRequest
{
    [Required, RegularExpression("^(admin|subscriber|public_user)$")]
    public string Role { get; set; } = string.Empty;
}

public class UpdateUserStatusRequest
{
    [Required, RegularExpression("^(active|inactive|banned)$")]
    public string Status { get; set; } = string.Empty;
}

public class UpdateRoleFeatureFlagsRequest
{
    public bool CanUseCustomApiKey { get; set; }

    public bool CanAccessAllExpressions { get; set; }

    public bool CanAccessAllAnimations { get; set; }

    public bool CanSelectAvatarModel { get; set; }

    public bool CanCustomizeVoice { get; set; }

    public bool CanAccessChatHistory { get; set; } = true;

    [Range(-1, int.MaxValue)]
    public int MaxConversationHistory { get; set; } = 10;

    [Range(-1, int.MaxValue)]
    public int MaxMessagesPerDay { get; set; } = 50;
}

public class CreateAvatarModelRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string FileUrl { get; set; } = string.Empty;

    public string? ThumbnailUrl { get; set; }

    [Required, RegularExpression("^(public_user|subscriber|admin)$")]
    public string MinRole { get; set; } = "public_user";

    public string? Description { get; set; }

    public bool IsDefault { get; set; }
}

public class UpdateAvatarModelRequest
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? ThumbnailUrl { get; set; }

    public string? MinRole { get; set; }

    public bool? IsDefault { get; set; }

    public bool? IsActive { get; set; }
}

public class CreateExpressionRequest
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required, RegularExpression("^(public_user|subscriber|admin)$")]
    public string MinRole { get; set; } = "public_user";
}

public class UpdateExpressionRequest
{
    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public string? MinRole { get; set; }

    public bool? IsActive { get; set; }
}

public class CreateAnimationRequest
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required, RegularExpression("^(public_user|subscriber|admin)$")]
    public string MinRole { get; set; } = "public_user";
}

public class UpdateAnimationRequest
{
    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public string? MinRole { get; set; }

    public bool? IsActive { get; set; }
}

public class UpdateSystemSettingRequest
{
    [Required]
    public string Value { get; set; } = string.Empty;
}
