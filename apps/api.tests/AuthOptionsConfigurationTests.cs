using Microsoft.Extensions.Configuration;
using RetroHiscore.Api.Infrastructure;
using RetroHiscore.Api.Options;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class AuthOptionsConfigurationTests
{
    [Fact]
    public void ApplySharedAdminDiscordAllowlist_ParsesCommaSeparatedIds()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AUTH_ADMIN_DISCORD_USER_IDS"] = " 444 , 555 ",
            })
            .Build();
        var options = new AuthOptions
        {
            AdminDiscordUserIds = ["legacy-admin"],
        };

        // Act
        AuthExtensions.ApplySharedAdminDiscordAllowlist(configuration, options);

        // Assert
        options.AdminDiscordUserIds.ShouldBe(["444", "555"]);
    }

    [Fact]
    public void ApplySignInServiceKey_UsesDedicatedEnvWhenPresent()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AUTH_SIGN_IN_SERVICE_KEY"] = "sign-in-key",
                ["AUTH_SECRET"] = "auth-secret",
            })
            .Build();
        var options = new AuthOptions();

        // Act
        AuthExtensions.ApplySignInServiceKey(configuration, options);

        // Assert
        options.SignInServiceKey.ShouldBe("sign-in-key");
    }

    [Fact]
    public void ResolveJwtSigningKey_FallsBackToAuthSecret()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AUTH_SECRET"] = "auth-secret",
            })
            .Build();

        // Act
        var key = AuthExtensions.ResolveJwtSigningKey(configuration);

        // Assert
        key.ShouldBe("auth-secret");
    }

    [Fact]
    public void ResolveWebOrigin_FallsBackToAuthUrl()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AUTH_URL"] = "https://hiscore.example.com",
            })
            .Build();

        // Act
        var origin = AuthExtensions.ResolveWebOrigin(configuration);

        // Assert
        origin.ShouldBe("https://hiscore.example.com");
    }

    [Fact]
    public void ApplySignInServiceKey_FallsBackToAuthSecret()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AUTH_SECRET"] = "auth-secret",
            })
            .Build();
        var options = new AuthOptions();

        // Act
        AuthExtensions.ApplySignInServiceKey(configuration, options);

        // Assert
        options.SignInServiceKey.ShouldBe("auth-secret");
    }
}
