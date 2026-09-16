using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Notifications;
using RetroHiscore.Api.Infrastructure;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class DiscordWebhookDispatchServiceTests
{
    [Fact]
    public async Task DispatchPendingAsync_SkipsOutbox_WhenNotReady()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new AppDbContext(options);
        var protector = new WebhookUrlProtector(new EphemeralDataProtectionProvider());
        var handler = new DiscordWebhookClientTests.RecordingHandlerPublic();
        var httpFactory = new DiscordWebhookClientTests.StubHttpClientFactoryPublic(handler);
        var renderer = new DiscordWebhookTemplateRenderer();
        var validator = new DiscordWebhookPayloadValidator();
        var webhookClient = new DiscordWebhookClient(
            httpFactory,
            validator,
            NullLogger<DiscordWebhookClient>.Instance);

        var webhookId = Guid.NewGuid();
        db.DiscordWebhookConfigs.Add(new DiscordWebhookConfig
        {
            Id = webhookId,
            Name = "Test",
            Enabled = true,
            WebhookUrlProtected = protector.Protect("https://discord.test/webhook"),
            DigestIntervalMinutes = 0,
            PayloadTemplateJson = DiscordWebhookDefaultTemplates.ForEventKind(
                DiscordNotificationEventKind.GameTracked),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            EventSubscriptions =
            [
                new DiscordWebhookEventSubscription
                {
                    WebhookConfigId = webhookId,
                    EventKind = DiscordNotificationEventKind.GameTracked
                }
            ]
        });

        db.NotificationOutbox.Add(new NotificationOutbox
        {
            EventKind = DiscordNotificationEventKind.GameTracked,
            PayloadJson = """{"raGameId":1}""",
            OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-5),
            ReadyAt = null
        });
        await db.SaveChangesAsync();

        var dispatch = new DiscordWebhookDispatchService(
            db,
            renderer,
            webhookClient,
            protector,
            NullLogger<DiscordWebhookDispatchService>.Instance);

        // Act
        await dispatch.DispatchPendingAsync();

        // Assert
        handler.RequestCount.ShouldBe(0);
        var outbox = await db.NotificationOutbox.SingleAsync();
        outbox.DispatchedAt.ShouldBeNull();
    }
}
