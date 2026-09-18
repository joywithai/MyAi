using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyAi.Application.Features.Avatar;

/// <summary>
/// Global 3D avatar scene configuration. Set by admins, read by everyone —
/// persisted as JSON in SystemSetting key "avatar_scene_config".
/// Mirrors the tunables of web/src/components/avatar/AvatarViewer.tsx.
/// </summary>
public sealed class AvatarSceneConfigDto
{
    public const string SettingKey = "avatar_scene_config";

    public SceneCameraDto Camera { get; set; } = new();

    public SceneModelDto Model { get; set; } = new();

    public SceneLightsDto Lights { get; set; } = new();

    /// <summary>Avatar panel height in px (240–720).</summary>
    public int CanvasHeight { get; set; } = 430;

    public sealed class SceneCameraDto
    {
        public double X { get; set; } = 0;

        public double Y { get; set; } = 1.35;

        public double Z { get; set; } = 2.1;

        public double LookAtX { get; set; } = 0;

        public double LookAtY { get; set; } = 1.05;

        public double LookAtZ { get; set; } = 0;

        /// <summary>Field of view in degrees (10–90).</summary>
        public double Fov { get; set; } = 30;
    }

    public sealed class SceneModelDto
    {
        public double X { get; set; } = 0;

        public double Y { get; set; } = 0;

        public double Z { get; set; } = 0;

        /// <summary>Yaw in radians (-π..π).</summary>
        public double RotationY { get; set; } = 0;

        /// <summary>Uniform scale (0.1–3).</summary>
        public double Scale { get; set; } = 1;
    }

    public sealed class SceneLightsDto
    {
        public double HemiIntensity { get; set; } = 1.1;

        public double DirX { get; set; } = 1.5;

        public double DirY { get; set; } = 2.5;

        public double DirZ { get; set; } = 2;

        /// <summary>Hex colour, e.g. "#9b83ff".</summary>
        public string DirColor { get; set; } = "#9b83ff";

        public double DirIntensity { get; set; } = 1.6;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public static AvatarSceneConfigDto Default() => new();

    /// <summary>Fills missing/invalid members with defaults after a partial deserialize.</summary>
    public static AvatarSceneConfigDto Normalize(AvatarSceneConfigDto? config)
    {
        var result = config ?? new AvatarSceneConfigDto();
        result.Camera ??= new SceneCameraDto();
        result.Model ??= new SceneModelDto();
        result.Lights ??= new SceneLightsDto();
        result.Lights.DirColor = string.IsNullOrWhiteSpace(result.Lights.DirColor)
            ? "#9b83ff"
            : result.Lights.DirColor;
        return result;
    }

    public static AvatarSceneConfigDto FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Default();
        }

        try
        {
            return Normalize(JsonSerializer.Deserialize<AvatarSceneConfigDto>(json, JsonOptions));
        }
        catch (JsonException)
        {
            return Default();
        }
    }

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    /// <summary>Clamps every value into a safe range so a bad admin input can't break the scene.</summary>
    public void Clamp()
    {
        const double MaxRadius = 10;

        Camera.X = Math.Clamp(Camera.X, -MaxRadius, MaxRadius);
        Camera.Y = Math.Clamp(Camera.Y, 0.05, 4);
        Camera.Z = Math.Clamp(Camera.Z, -MaxRadius, MaxRadius);
        Camera.LookAtX = Math.Clamp(Camera.LookAtX, -MaxRadius, MaxRadius);
        Camera.LookAtY = Math.Clamp(Camera.LookAtY, 0, 3);
        Camera.LookAtZ = Math.Clamp(Camera.LookAtZ, -MaxRadius, MaxRadius);
        Camera.Fov = Math.Clamp(Camera.Fov, 10, 90);

        Model.X = Math.Clamp(Model.X, -MaxRadius, MaxRadius);
        Model.Y = Math.Clamp(Model.Y, -MaxRadius, MaxRadius);
        Model.Z = Math.Clamp(Model.Z, -MaxRadius, MaxRadius);
        Model.RotationY = Math.Clamp(Model.RotationY, -Math.PI, Math.PI);
        Model.Scale = Math.Clamp(Model.Scale, 0.1, 3);

        Lights.HemiIntensity = Math.Clamp(Lights.HemiIntensity, 0, 5);
        Lights.DirX = Math.Clamp(Lights.DirX, -MaxRadius, MaxRadius);
        Lights.DirY = Math.Clamp(Lights.DirY, 0.1, 10);
        Lights.DirZ = Math.Clamp(Lights.DirZ, -MaxRadius, MaxRadius);
        Lights.DirIntensity = Math.Clamp(Lights.DirIntensity, 0, 5);

        if (!System.Text.RegularExpressions.Regex.IsMatch(Lights.DirColor, "^#[0-9a-fA-F]{6}$"))
        {
            Lights.DirColor = "#9b83ff";
        }

        CanvasHeight = (int)Math.Clamp(CanvasHeight, 240, 720);
    }
}
