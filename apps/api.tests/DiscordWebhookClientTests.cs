using System.Net;
using RetroHiscore.Api.Features.Notifications;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class DiscordWebhookClientTests
{
    [Fact]
    public async Task PostAsync_SendsRenderedPayload_WhenSuccessful()
    {
        // Arrange
        var handler = new RecordingHandler();
        var httpClientFactory = new StubHttpClientFactory(handler);
        var validator = new DiscordWebhookPayloadValidator();
        var client = new DiscordWebhookClient(
            httpClientFactory,
            validator,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<DiscordWebhookClient>.Instance);

        var payload = """{"embeds":[{"title":"Hello","description":"World"}]}""";

        // Act
        var ok = await client.PostAsync("https://discord.test/webhook", payload);

        // Assert
        ok.ShouldBeTrue();
        handler.RequestCount.ShouldBe(1);
        handler.LastPayload.ShouldContain("Hello");
    }

    [Fact]
    public async Task PostAsync_ReturnsFalse_WhenWebhookFails()
    {
        // Arrange
        var handler = new RecordingHandler { FailRequests = true };
        var httpClientFactory = new StubHttpClientFactory(handler);
        var validator = new DiscordWebhookPayloadValidator();
        var client = new DiscordWebhookClient(
            httpClientFactory,
            validator,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<DiscordWebhookClient>.Instance);

        // Act
        var ok = await client.PostAsync("https://discord.test/webhook", """{"embeds":[{"title":"x"}]}""");

        // Assert
        ok.ShouldBeFalse();
        handler.RequestCount.ShouldBe(1);
    }

    public sealed class RecordingHandlerPublic : HttpMessageHandler
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

    public sealed class StubHttpClientFactoryPublic(RecordingHandlerPublic handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler);
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
        public HttpClient CreateClient(string name) => new(handler);
    }
}
