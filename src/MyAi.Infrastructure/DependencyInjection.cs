using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Infrastructure.AI.Factories;
using MyAi.Infrastructure.AI.OpenRouter;
using MyAi.Infrastructure.Caching.Memory;
using MyAi.Infrastructure.Caching.Redis;
using MyAi.Infrastructure.Email;
using MyAi.Infrastructure.Events.Publishers;
using MyAi.Infrastructure.Identity;
using MyAi.Infrastructure.Identity.Configuration;
using MyAi.Infrastructure.Persistence;
using MyAi.Infrastructure.Persistence.Decorators;
using MyAi.Infrastructure.Persistence.Interceptors;
using MyAi.Infrastructure.Persistence.Repositories;
using MyAi.Infrastructure.Storage.Blob;
using MyAi.Infrastructure.Storage.Factories;
using MyAi.Infrastructure.Storage.Local;
using MyAi.Infrastructure.TTS.EdgeTts;
using MyAi.Infrastructure.TTS.Factories;
using MyAi.Infrastructure.TTS.Shared;

namespace MyAi.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers persistence, caching, identity, AI, TTS, storage, email and events.</summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // ── Persistence ────────────────────────────────────────────────────────
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<IAvatarModelRepository, AvatarModelRepository>();
        services.AddScoped<IExpressionRepository, ExpressionRepository>();
        services.AddScoped<IAnimationRepository, AnimationRepository>();
        services.AddScoped<ICustomAiConfigRepository, CustomAiConfigRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IFeatureFlagsRepository, FeatureFlagsRepository>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // ── Caching: Redis when configured, in-memory fallback otherwise ──────
        var redisConnectionString = configuration.GetConnectionString("Redis");
        services.AddMemoryCache();
        services.AddSingleton<RedisConfiguration>(_ => new RedisConfiguration
        {
            ConnectionString = redisConnectionString ?? string.Empty
        });
        services.AddSingleton<RedisConnectionFactory>();

        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddSingleton<ICacheService, MemoryCacheService>();
        }

        // ── Identity ───────────────────────────────────────────────────────────
        services.Configure<JwtConfiguration>(configuration.GetSection(JwtConfiguration.SectionName));
        services.AddSingleton<JwtSigningKeyProvider>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IApiKeyEncryptionService, ApiKeyEncryptionService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // ── AI ─────────────────────────────────────────────────────────────────
        var openRouterApiKey = configuration["OpenRouter:ApiKey"];
        var openRouterDefaultModel = configuration["OpenRouter:DefaultModel"] ?? "google/gemini-2.0-flash-001:free";

        services.AddHttpClient<OpenRouterHttpClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
        });
        services.AddSingleton<OpenRouterPromptBuilder>();
        services.AddSingleton<OpenRouterResponseParser>();
        services.AddSingleton<IOpenRouterKeyTester, OpenRouterKeyTester>();
        services.AddSingleton<IAiProviderFactory>(sp => new AiProviderFactory(
            sp,
            sp.GetRequiredService<OpenRouterHttpClient>(),
            sp.GetRequiredService<OpenRouterPromptBuilder>(),
            sp.GetRequiredService<OpenRouterResponseParser>(),
            openRouterApiKey,
            openRouterDefaultModel,
            sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AiProviderFactory>>()));

        // ── TTS ────────────────────────────────────────────────────────────────
        services.AddSingleton<EdgeTtsMessageBuilder>();
        services.AddSingleton<EdgeTtsWebSocketClient>();
        services.AddSingleton<VoiceSelector>();
        services.AddSingleton<ITtsProviderFactory, TtsProviderFactory>();

        // ── Storage ────────────────────────────────────────────────────────────
        services.Configure<BlobStorageConfiguration>(configuration.GetSection(BlobStorageConfiguration.SectionName));
        services.AddSingleton<IStorageService, StorageServiceFactory>();

        // ── Email ──────────────────────────────────────────────────────────────
        services.Configure<EmailConfiguration>(configuration.GetSection(EmailConfiguration.SectionName));
        services.AddSingleton<IEmailService, SmtpEmailService>();

        // ── Events ─────────────────────────────────────────────────────────────
        services.AddScoped<IEventPublisher, DomainEventPublisher>();

        // ── Background jobs ────────────────────────────────────────────────────
        services.AddScoped<AudioCleanupJob>();
        services.AddScoped<ExpiredSubscriptionJob>();
        services.AddScoped<RefreshTokenCleanupJob>();

        return services;
    }
}
