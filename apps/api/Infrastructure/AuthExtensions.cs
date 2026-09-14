using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Infrastructure;

public static class AuthExtensions
{
    public const string AllowlistedDiscordUserPolicy = "AllowlistedDiscordUser";
    public const string AdminDiscordUserPolicy = "AdminDiscordUser";

    public static IServiceCollection AddRetroHiscoreAuth(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isTesting)
    {
        services.Configure<AuthOptions>(options =>
        {
            configuration.GetSection(AuthOptions.SectionName).Bind(options);
            ApplySharedDiscordAllowlist(configuration, options);
            ApplySharedAdminDiscordAllowlist(configuration, options);
        });

        var signingKey = configuration[$"{AuthOptions.SectionName}:JwtSigningKey"];
        if (!isTesting && string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException(
                $"{AuthOptions.SectionName}:JwtSigningKey is required outside the Testing environment.");
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
                var webOrigin = configuration[$"{AuthOptions.SectionName}:WebOrigin"]
                    ?? "http://localhost:18321";
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

    public static void ApplySharedDiscordAllowlist(IConfiguration configuration, AuthOptions options)
    {
        var sharedAllowlist = configuration["AUTH_ALLOWED_DISCORD_USER_IDS"];
        if (string.IsNullOrWhiteSpace(sharedAllowlist))
        {
            return;
        }

        options.AllowedDiscordUserIds = sharedAllowlist
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
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
}
