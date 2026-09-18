using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyAi.Api.Filters;
using MyAi.Application.Common.Constants;
using MyAi.Infrastructure.Identity;

namespace MyAi.Api.Extensions;

/// <summary>
/// Wires the JWT bearer validation to the shared RS256 key
/// (JwtSigningKeyProvider — the same key the token generator signs with).
/// </summary>
public class JwtBearerPostConfigurer : IPostConfigureOptions<JwtBearerOptions>
{
    private readonly IServiceProvider _serviceProvider;

    public JwtBearerPostConfigurer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        var keyProvider = _serviceProvider.GetRequiredService<JwtSigningKeyProvider>();
        options.TokenValidationParameters.IssuerSigningKeyResolver =
            (_, _, _, _) => keyProvider.GetValidationKeys();
    }
}

public static class ServiceCollectionExtensions
{
    /// <summary>Application layer: MediatR, validators, AutoMapper, behaviours.</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Qualified call — avoids extension-method name clash with this class.
        return MyAi.Application.DependencyInjection.AddApplicationServices(services);
    }

    /// <summary>Infrastructure layer: EF Core, Redis, identity, AI, TTS, storage, email.</summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        return MyAi.Infrastructure.DependencyInjection.AddInfrastructureServices(services, configuration);
    }

    /// <summary>API layer: controllers, Swagger, JWT auth, CORS, Hangfire, filters.</summary>
    public static IServiceCollection AddApiServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidateModelFilter>();
        });

        services.AddScoped<AuthorizeResourceFilter>();
        services.AddScoped<MyAi.Application.Common.Interfaces.IConversationOwnershipChecker,
            MyAi.Infrastructure.Persistence.ConversationOwnershipChecker>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MyAi API",
                Version = "v1",
                Description = "3D AI Avatar chatbot API — OpenRouter AI, Edge TTS, VRM avatar, role-based access."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT access token."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtSection = configuration.GetSection("Jwt");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSection["Issuer"] ?? "MyAi",
                    ValidateAudience = true,
                    ValidAudience = jwtSection["Audience"] ?? "MyAi-Web",
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigurer>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyConstants.AdminOnly, policy =>
                policy.RequireRole(RoleConstants.Admin));
            options.AddPolicy(PolicyConstants.SubscriberOrAdmin, policy =>
                policy.RequireRole(RoleConstants.Subscriber, RoleConstants.Admin));
        });

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy("Web", corsPolicy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    corsPolicy.WithOrigins(allowedOrigins)
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
                else
                {
                    corsPolicy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        services.AddHangfire(hangfire => hangfire.UseMemoryStorage());
        services.AddHangfireServer();

        return services;
    }
}
