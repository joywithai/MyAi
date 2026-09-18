using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Interfaces;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;
using MyAi.Domain.ValueObjects;

namespace MyAi.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds default data on startup: feature flags for the three roles, system settings,
/// 12 expressions, 6 animations, default avatar model, subscription plans and the
/// bootstrap admin account (from SeedAdmin config).
/// All inserts are idempotent.
/// </summary>
public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;

    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(string? adminEmail, string? adminPassword, string? adminDisplayName, CancellationToken ct = default)
    {
        await SeedFeatureFlagsAsync(ct);
        await SeedSystemSettingsAsync(ct);
        await SeedExpressionsAsync(ct);
        await SeedAnimationsAsync(ct);
        await SeedAvatarModelsAsync(ct);
        await SeedSubscriptionPlansAsync(ct);
        await SeedAdminUserAsync(adminEmail, adminPassword, adminDisplayName, ct);

        _logger.LogInformation("Database seeding completed");
    }

    private async Task SeedFeatureFlagsAsync(CancellationToken ct)
    {
        foreach (var role in new[] { UserRole.Admin, UserRole.Subscriber, UserRole.PublicUser })
        {
            var exists = await _context.RoleFeatureFlags.AnyAsync(f => f.Role == role, ct);

            if (!exists)
            {
                _context.RoleFeatureFlags.Add(RoleFeatureFlags.CreateDefault(role));
                _logger.LogInformation("Seeded feature flags for role {Role}", role);
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedSystemSettingsAsync(CancellationToken ct)
    {
        var defaults = new Dictionary<string, (string Value, string Description)>
        {
            ["default_ai_model"] = ("google/gemini-2.0-flash-001:free", "Default OpenRouter model"),
            ["default_ai_provider"] = ("openrouter", "Default AI provider name"),
            ["payment_provider"] = ("demo", "Active payment provider (demo|stripe|sslcommerz)"),
            ["maintenance_mode"] = ("false", "Global maintenance switch"),
            ["registration_enabled"] = ("true", "Allow new registrations"),
            ["max_message_length"] = ("500", "Maximum chat message length in characters"),
            ["avatar_scene_config"] =
                (@"{""camera"":{""x"":0,""y"":1.35,""z"":2.1,""lookAtX"":0,""lookAtY"":1.05,""lookAtZ"":0,""fov"":30}," +
                 @"""model"":{""x"":0,""y"":0,""z"":0,""rotationY"":0,""scale"":1}," +
                 @"""lights"":{""hemiIntensity"":1.1,""dirX"":1.5,""dirY"":2.5,""dirZ"":2,""dirColor"":""#9b83ff"",""dirIntensity"":1.6}," +
                 @"""canvasHeight"":430}",
                 "Global avatar scene configuration (JSON) — edited from the admin panel")
        };

        foreach (var (key, (value, description)) in defaults)
        {
            var exists = await _context.SystemSettings.AnyAsync(s => s.Key == key, ct);

            if (!exists)
            {
                _context.SystemSettings.Add(SystemSetting.Create(key, value, description));
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedExpressionsAsync(CancellationToken ct)
    {
        var expressions = new List<(string Name, string Display, UserRole MinRole)>
        {
            ("NEUTRAL", "Neutral", UserRole.PublicUser),
            ("HAPPY", "Happy", UserRole.PublicUser),
            ("SAD", "Sad", UserRole.PublicUser),
            ("SURPRISED", "Surprised", UserRole.PublicUser),
            ("ANGRY", "Angry", UserRole.Subscriber),
            ("RELAXED", "Relaxed", UserRole.Subscriber),
            ("EXCITED", "Excited", UserRole.Subscriber),
            ("CONFUSED", "Confused", UserRole.Subscriber),
            ("THOUGHTFUL", "Thoughtful", UserRole.Subscriber),
            ("CONCERNED", "Concerned", UserRole.Subscriber),
            ("FRIENDLY", "Friendly", UserRole.Subscriber),
            ("SERIOUS", "Serious", UserRole.Subscriber)
        };

        var index = 0;

        foreach (var (name, display, minRole) in expressions)
        {
            var exists = await _context.Expressions.AnyAsync(e => e.Name == name, ct);

            if (!exists)
            {
                var expression = Expression.Create(name, display, minRole);
                expression.SetSortOrder(index++);
                _context.Expressions.Add(expression);
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedAnimationsAsync(CancellationToken ct)
    {
        var animations = new List<(string Name, string Display, string Description, UserRole MinRole)>
        {
            ("breathing", "Breathing", "Subtle chest rise — always alive", UserRole.PublicUser),
            ("head-sway", "Head Sway", "Gentle side-to-side head motion", UserRole.Subscriber),
            ("shoulder-bob", "Shoulder Bob", "Shoulders rise/fall with breathing", UserRole.Subscriber),
            ("hand-gesture", "Hand Gesture", "Occasional natural hand gestures", UserRole.Subscriber),
            ("thinking-pose", "Thinking Pose", "Head tilt + raised arm while AI thinks", UserRole.Subscriber),
            ("wave", "Wave", "Friendly greeting wave", UserRole.Subscriber)
        };

        var index = 0;

        foreach (var (name, display, description, minRole) in animations)
        {
            var exists = await _context.Animations.AnyAsync(a => a.Name == name, ct);

            if (!exists)
            {
                var animation = Animation.Create(name, display, minRole);
                animation.SetDescription(description);
                animation.SetSortOrder(index++);
                _context.Animations.Add(animation);
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedAvatarModelsAsync(CancellationToken ct)
    {
        var hasDefault = await _context.AvatarModels.AnyAsync(m => m.IsDefault && m.IsActive, ct);

        if (!hasDefault)
        {
            var model = AvatarModel.Create(
                "AI Assistant Avatar",
                "/models/AIAssistantAvatar.vrm",
                null,
                UserRole.PublicUser);

            model.SetDescription("Bundled VRM 1.0 avatar (AIAssistantAvatar.vrm)");
            model.MakeDefault();
            _context.AvatarModels.Add(model);
            _logger.LogInformation("Seeded default avatar model: AIAssistantAvatar.vrm");
        }

        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedSubscriptionPlansAsync(CancellationToken ct)
    {
        if (await _context.SubscriptionPlans.AnyAsync(ct))
        {
            return;
        }

        var monthly = SubscriptionPlan.Create(
            "Basic Monthly", UserRole.Subscriber, BillingCycle.Monthly, 500m, "BDT");
        monthly.SetDescription("All premium features for 30 days");
        monthly.SetFeatures(new List<string>
        {
            "Custom AI API Key",
            "Full 12 Expression Access",
            "All Animations",
            "All Avatar Models",
            "Voice Speed & Pitch Control",
            "Unlimited History",
            "500 messages/day"
        });
        monthly.SetSortOrder(0);

        var yearly = SubscriptionPlan.Create(
            "Pro Yearly", UserRole.Subscriber, BillingCycle.Yearly, 5000m, "BDT");
        yearly.SetDescription("Best value — 2 months free");
        yearly.SetFeatures(new List<string>
        {
            "Everything in Basic Monthly",
            "Priority AI response",
            "2 months free vs monthly",
            "500 messages/day"
        });
        yearly.SetSortOrder(1);

        _context.SubscriptionPlans.AddRange(monthly, yearly);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded subscription plans");
    }

    private async Task SeedAdminUserAsync(
        string? adminEmail, string? adminPassword, string? adminDisplayName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        var email = Email.Create(adminEmail);
        var exists = await _context.Users.AnyAsync(u => u.Email == email, ct);

        if (exists)
        {
            return;
        }

        // The password hash service lives in DI; resolve it from the context's service provider.
        var hasher = _context.GetInfrastructure().GetService(typeof(IPasswordHasher)) as IPasswordHasher;

        if (hasher is null)
        {
            _logger.LogWarning("Cannot seed admin: password hasher unavailable");
            return;
        }

        var admin = User.Create(adminEmail, hasher.Hash(adminPassword), adminDisplayName ?? "Administrator");
        admin.ChangeRole(UserRole.Admin);
        admin.MarkEmailVerified();

        // ChangeRole fires an event on a brand-new entity; clear to avoid double handling.
        admin.ClearDomainEvents();

        _context.Users.Add(admin);
        _context.UserSettings.Add(UserSettings.CreateDefault(admin.Id));
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Seeded bootstrap admin account: {Email}", adminEmail);
    }
}
