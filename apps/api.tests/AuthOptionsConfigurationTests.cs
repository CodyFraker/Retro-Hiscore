using Microsoft.Extensions.Configuration;
using RetroHiscore.Api.Infrastructure;
using RetroHiscore.Api.Options;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class AuthOptionsConfigurationTests
{
    [Fact]
    public void ApplySharedDiscordAllowlist_ParsesCommaSeparatedIds()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AUTH_ALLOWED_DISCORD_USER_IDS"] = " 111 , 222 ,333 ",
            })
            .Build();
        var options = new AuthOptions
        {
            AllowedDiscordUserIds = ["legacy-id"],
        };

        // Act
        AuthExtensions.ApplySharedDiscordAllowlist(configuration, options);

        // Assert
        options.AllowedDiscordUserIds.ShouldBe(["111", "222", "333"]);
    }

    [Fact]
    public void ApplySharedDiscordAllowlist_LeavesSectionValuesWhenEnvMissing()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var options = new AuthOptions
        {
            AllowedDiscordUserIds = ["legacy-id"],
        };

        // Act
        AuthExtensions.ApplySharedDiscordAllowlist(configuration, options);

        // Assert
        options.AllowedDiscordUserIds.ShouldBe(["legacy-id"]);
    }

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
}
