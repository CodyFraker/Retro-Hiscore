using System.Net;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Features.Dashboard;
using RetroHiscore.Api.Features.Notifications;
using RetroHiscore.Api.Options;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class DiscordNotificationTests
{
    [Fact]
    public async Task NotifyActivityAsync_SendsWebhook_WhenRankChanges()
    {
        // Arrange
        var handler = new RecordingHandler();
        var httpClientFactory = new StubHttpClientFactory(handler);
        var options = Microsoft.Extensions.Options.Options.Create(new NotificationOptions
        {
            DiscordWebhookUrl = "https://discord.test/webhook",
            NotifyOnRankChange = true
        });
        var service = new DiscordNotificationService(
            httpClientFactory,
            options,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<DiscordNotificationService>.Instance);

        var activity = new List<ActivityItemDto>
        {
            new(
                Guid.NewGuid(),
                "ShrimpPoboy",
                "Shrimp",
                null,
                38130,
                "Pinball",
                3813001,
                "High Score",
                50,
                1,
                "150")
        };

        // Act
        await service.NotifyActivityAsync(activity);

        // Assert
        handler.RequestCount.ShouldBe(1);
        handler.LastPayload.ShouldContain("Shrimp");
        handler.LastPayload.ShouldContain("Pinball");
    }

    [Fact]
    public async Task NotifyActivityAsync_SkipsWebhook_WhenUrlNotConfigured()
    {
        // Arrange
        var handler = new RecordingHandler();
        var httpClientFactory = new StubHttpClientFactory(handler);
        var options = Microsoft.Extensions.Options.Options.Create(new NotificationOptions { DiscordWebhookUrl = null });
        var service = new DiscordNotificationService(
            httpClientFactory,
            options,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<DiscordNotificationService>.Instance);

        var activity = new List<ActivityItemDto>
        {
            new(
                Guid.NewGuid(),
                "ShrimpPoboy",
                "Shrimp",
                null,
                38130,
                "Pinball",
                3813001,
                "High Score",
                50,
                1,
                "150")
        };

        // Act
        await service.NotifyActivityAsync(activity);

        // Assert
        handler.RequestCount.ShouldBe(0);
    }

    [Fact]
    public async Task NotifyActivityAsync_DoesNotThrow_WhenWebhookFails()
    {
        // Arrange
        var handler = new RecordingHandler { FailRequests = true };
        var httpClientFactory = new StubHttpClientFactory(handler);
        var options = Microsoft.Extensions.Options.Options.Create(new NotificationOptions
        {
            DiscordWebhookUrl = "https://discord.test/webhook",
            NotifyOnRankChange = true
        });
        var service = new DiscordNotificationService(
            httpClientFactory,
            options,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<DiscordNotificationService>.Instance);

        var activity = new List<ActivityItemDto>
        {
            new(
                Guid.NewGuid(),
                "ShrimpPoboy",
                "Shrimp",
                null,
                38130,
                "Pinball",
                3813001,
                "High Score",
                50,
                1,
                "150")
        };

        // Act
        await service.NotifyActivityAsync(activity);

        // Assert
        handler.RequestCount.ShouldBe(1);
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }
        public string LastPayload { get; private set; } = "";
        public bool FailRequests { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            LastPayload = request.Content is null
                ? ""
                : await request.Content.ReadAsStringAsync(cancellationToken);

            if (FailRequests)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }

    private sealed class StubHttpClientFactory(RecordingHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
            => new(handler);
    }
}
