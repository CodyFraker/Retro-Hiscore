using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Notifications;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class DiscordWebhookTemplateRendererTests
{
    private readonly DiscordWebhookTemplateRenderer _renderer = new();
    private readonly DiscordWebhookPayloadValidator _validator = new();

    [Fact]
    public void Render_ReplacesShortTokens_InEmbedDescription()
    {
        // Arrange
        var template = """
            {"embeds":[{"title":"{{usr}}","description":"{{gt}} — {{lb}}"}]}
            """;
        var payload = DiscordWebhookSamplePayloads.BuildJson(DiscordNotificationEventKind.LeaderboardFriendOvertake);

        // Act
        var rendered = _renderer.Render(template, DiscordNotificationEventKind.LeaderboardFriendOvertake, payload);

        // Assert
        rendered.ShouldContain("Sample Player");
        rendered.ShouldContain("Pinball");
        rendered.ShouldContain("High Score");
    }

    [Fact]
    public void ValidateTemplate_RejectsUnknownToken()
    {
        // Arrange
        var template = """{"embeds":[{"description":"{{zzz}}"}]}""";
        var allowed = DiscordTokenCatalog.AllowedCodesForKinds([DiscordNotificationEventKind.GameTracked]);

        // Act
        var act = () => _validator.ValidateTemplate(template, allowed);

        // Assert
        Should.Throw<DiscordWebhookTemplateValidationException>(act);
    }

    [Fact]
    public void Render_ReplacesGameTrackedTokens_AfterOutboxRoundTrip()
    {
        // Arrange
        var payload = new GameTrackedNotificationPayload(
            8010,
            "Pinball",
            "Arcade",
            12,
            50,
            5000,
            DateTimeOffset.UtcNow);
        var json = System.Text.Json.JsonSerializer.Serialize(payload, NotificationPayloadJson.Options);
        var template = """{"embeds":[{"title":"{{gt}}","description":"{{cn}} · {{nl}} boards"}]}""";

        // Act
        var rendered = _renderer.Render(template, DiscordNotificationEventKind.GameTracked, json);

        // Assert
        rendered.ShouldContain("Pinball");
        rendered.ShouldContain("Arcade");
        rendered.ShouldContain("12");
    }

    [Fact]
    public void ValidateTemplate_RequiresEmbedsArray()
    {
        // Arrange
        var template = """{"content":"hello"}""";
        var allowed = DiscordTokenCatalog.AllowedCodesForKinds([DiscordNotificationEventKind.GameTracked]);

        // Act
        var act = () => _validator.ValidateTemplate(template, allowed);

        // Assert
        Should.Throw<DiscordWebhookTemplateValidationException>(act);
    }
}
