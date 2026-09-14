using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Infrastructure;

public static class AuthExtensions
{
    public const string AllowlistedDiscordUserPolicy = "AllowlistedDiscordUser";
    public const string AdminDiscordUserPolicy = "AdminDiscordUser";
    public const string SignInServiceKeyHeader = "X-SignIn-Service-Key";

    public static IServiceCollection AddRetroHiscoreAuth(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isTesting)
    {
        services.Configure<AuthOptions>(options =>
        {
            configuration.GetSection(AuthOptions.SectionName).Bind(options);
            ApplyWebOrigin(configuration, options);
            ApplySharedAdminDiscordAllowlist(configuration, options);
            ApplySignInServiceKey(configuration, options);
        });

        var signingKey = ResolveJwtSigningKey(configuration);
        if (!isTesting && string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException(
                "AUTH_SECRET (or Auth:JwtSigningKey) is required outside the Testing environment.");
        }

        if (!isTesting)
        {
            var adminIds = configuration["AUTH_ADMIN_DISCORD_USER_IDS"];
            if (string.IsNullOrWhiteSpace(adminIds))
            {
                throw new InvalidOperationException("AUTH_ADMIN_DISCORD_USER_IDS must contain at least one Discord user ID.");
            }
        }

        if (string.IsNullOrWhiteSpace(signingKey))
        {
            signingKey = "test-signing-key-for-integration-tests-only";
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "name"
                };
            });

        services.AddSingleton<IAuthorizationHandler, AllowlistedDiscordUserAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, AdminDiscordUserAuthorizationHandler>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AllowlistedDiscordUserPolicy, policy =>
                policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new AllowlistedDiscordUserRequirement()));

            options.AddPolicy(AdminDiscordUserPolicy, policy =>
                policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new AllowlistedDiscordUserRequirement(), new AdminDiscordUserRequirement()));
        });

        return services;
    }

    public static IServiceCollection AddRetroHiscoreCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                var webOrigin = ResolveWebOrigin(configuration);
                policy
                    .WithOrigins(webOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    public static RouteHandlerBuilder RequireApiAuth(this RouteHandlerBuilder builder)
        => builder.RequireAuthorization(AllowlistedDiscordUserPolicy);

    public static RouteHandlerBuilder RequireAdmin(this RouteHandlerBuilder builder)
        => builder.RequireAuthorization(AdminDiscordUserPolicy);

    public static string? ResolveJwtSigningKey(IConfiguration configuration)
    {
        var fromSection = configuration[$"{AuthOptions.SectionName}:JwtSigningKey"];
        if (!string.IsNullOrWhiteSpace(fromSection))
        {
            return fromSection;
        }

        return configuration["AUTH_SECRET"];
    }

    public static string ResolveWebOrigin(IConfiguration configuration)
    {
        var fromSection = configuration[$"{AuthOptions.SectionName}:WebOrigin"];
        if (!string.IsNullOrWhiteSpace(fromSection))
        {
            return fromSection;
        }

        var authUrl = configuration["AUTH_URL"];
        if (!string.IsNullOrWhiteSpace(authUrl))
        {
            return authUrl;
        }

        return "http://localhost:18321";
    }

    public static void ApplyWebOrigin(IConfiguration configuration, AuthOptions options)
    {
        var resolved = ResolveWebOrigin(configuration);
        if (!string.IsNullOrWhiteSpace(resolved))
        {
            options.WebOrigin = resolved;
        }
    }

    public static void ApplySharedAdminDiscordAllowlist(IConfiguration configuration, AuthOptions options)
    {
        var sharedAdmins = configuration["AUTH_ADMIN_DISCORD_USER_IDS"];
        if (string.IsNullOrWhiteSpace(sharedAdmins))
        {
            return;
        }

        options.AdminDiscordUserIds = sharedAdmins
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    public static void ApplySignInServiceKey(IConfiguration configuration, AuthOptions options)
    {
        var key = configuration["AUTH_SIGN_IN_SERVICE_KEY"];
        if (!string.IsNullOrWhiteSpace(key))
        {
            options.SignInServiceKey = key;
            return;
        }

        var authSecret = configuration["AUTH_SECRET"];
        if (!string.IsNullOrWhiteSpace(authSecret))
        {
            options.SignInServiceKey = authSecret;
        }
    }

    public static bool IsSignInServiceAuthorized(HttpRequest request, AuthOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.SignInServiceKey))
        {
            return false;
        }

        return request.Headers.TryGetValue(SignInServiceKeyHeader, out var header)
            && string.Equals(header.ToString(), options.SignInServiceKey, StringComparison.Ordinal);
    }
}
