using RetroHiscore.Api.Features.Notifications;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class DiscordWebhookUrlValidatorTests
{
    [Fact]
    public void Validate_AllowsDiscordWebhookUrl()
    {
        // Arrange
        var url = "https://discord.com/api/webhooks/1234567890/abcdefghijklmnopqrstuvwxyz";

        // Act & Assert
        Should.NotThrow(() => DiscordWebhookUrlValidator.Validate(url));
    }

    [Fact]
    public void Validate_RejectsHttpUrl()
    {
        // Arrange
        var url = "http://discord.com/api/webhooks/1234567890/token";

        // Act & Assert
        Should.Throw<DiscordWebhookValidationException>(() => DiscordWebhookUrlValidator.Validate(url));
    }

    [Fact]
    public void Validate_RejectsNonDiscordHost()
    {
        // Arrange
        var url = "https://example.com/api/webhooks/1234567890/token";

        // Act & Assert
        Should.Throw<DiscordWebhookValidationException>(() => DiscordWebhookUrlValidator.Validate(url));
    }

    [Fact]
    public void Validate_RejectsInternalIpTarget()
    {
        // Arrange
        var url = "https://169.254.169.254/api/webhooks/1/token";

        // Act & Assert
        Should.Throw<DiscordWebhookValidationException>(() => DiscordWebhookUrlValidator.Validate(url));
    }
}
