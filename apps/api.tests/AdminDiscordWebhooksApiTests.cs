using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Notifications;
using RetroHiscore.Api.Infrastructure;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class AdminDiscordWebhooksApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public AdminDiscordWebhooksApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAdminDiscordWebhooks_WithNonAdmin_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            AuthTestHelper.CreateToken(AuthTestHelper.SecondAllowedDiscordUserId));

        // Act
        var response = await client.GetAsync("/api/admin/discord-webhooks");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostAdminDiscordWebhook_WithAdmin_CreatesWebhook()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);
        var body = new UpsertDiscordWebhookRequest(
            "Leaderboard",
            true,
            "https://discord.com/api/webhooks/1/token",
            10,
            DiscordWebhookDefaultTemplates.ForEventKind(DiscordNotificationEventKind.LeaderboardFriendOvertake),
            ["LeaderboardFriendOvertake"],
            null);

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/discord-webhooks", body);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<AdminDiscordWebhookDetailDto>();
        created.ShouldNotBeNull();
        created.WebhookUrlMasked.ShouldContain("****");
        created.EventKinds.ShouldContain("LeaderboardFriendOvertake");
    }

    [Fact]
    public async Task PostAdminDiscordWebhook_WithUnknownToken_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);
        var body = new UpsertDiscordWebhookRequest(
            "Bad",
            true,
            "https://discord.com/api/webhooks/1/token",
            10,
            """{"embeds":[{"description":"{{zzz}}"}]}""",
            ["GameTracked"],
            null);

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/discord-webhooks", body);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTokenCatalog_ReturnsTokensPerEvent()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.GetAsync("/api/admin/discord-webhooks/token-catalog");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var catalog = await response.Content.ReadFromJsonAsync<DiscordTokenCatalogDto>();
        catalog.ShouldNotBeNull();
        catalog.Events.ShouldContain(e => e.EventKind == nameof(DiscordNotificationEventKind.GameTracked));
    }

    [Fact]
    public async Task DispatchService_MarksOutboxDelivered_WhenWebhookConfigured()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RetroHiscore.Api.Data.AppDbContext>();
        var protector = scope.ServiceProvider.GetRequiredService<IWebhookUrlProtector>();
        var handler = new DiscordWebhookClientTests.RecordingHandlerPublic();
        var httpFactory = new DiscordWebhookClientTests.StubHttpClientFactoryPublic(handler);
        var renderer = new DiscordWebhookTemplateRenderer();
        var validator = new DiscordWebhookPayloadValidator();
        var webhookClient = new DiscordWebhookClient(
            httpFactory,
            validator,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<DiscordWebhookClient>.Instance);

        var webhookId = Guid.NewGuid();
        db.DiscordWebhookConfigs.Add(new DiscordWebhookConfig
        {
            Id = webhookId,
            Name = "Test",
            Enabled = true,
            WebhookUrlProtected = protector.Protect("https://discord.test/webhook"),
            DigestIntervalMinutes = 0,
            PayloadTemplateJson = DiscordWebhookDefaultTemplates.ForEventKind(
                DiscordNotificationEventKind.LeaderboardFriendOvertake),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            EventSubscriptions =
            [
                new DiscordWebhookEventSubscription
                {
                    WebhookConfigId = webhookId,
                    EventKind = DiscordNotificationEventKind.LeaderboardFriendOvertake
                }
            ]
        });

        db.NotificationOutbox.Add(new NotificationOutbox
        {
            EventKind = DiscordNotificationEventKind.LeaderboardFriendOvertake,
            PayloadJson = DiscordWebhookSamplePayloads.BuildJson(DiscordNotificationEventKind.LeaderboardFriendOvertake),
            OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            ReadyAt = DateTimeOffset.UtcNow.AddMinutes(-1)
        });
        await db.SaveChangesAsync();

        var dispatch = new DiscordWebhookDispatchService(
            db,
            renderer,
            webhookClient,
            protector,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<DiscordWebhookDispatchService>.Instance);

        // Act
        await dispatch.DispatchPendingAsync();

        // Assert
        handler.RequestCount.ShouldBe(1);
        var delivery = await db.NotificationOutboxDeliveries.SingleAsync();
        delivery.DispatchedAt.ShouldNotBeNull();
        var outbox = await db.NotificationOutbox.SingleAsync();
        outbox.DispatchedAt.ShouldNotBeNull();
    }
}
