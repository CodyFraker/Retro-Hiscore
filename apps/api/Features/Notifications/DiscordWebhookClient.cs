using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace RetroHiscore.Api.Features.Notifications;

public interface IDiscordWebhookClient
{
    Task<bool> PostAsync(string webhookUrl, string renderedPayloadJson, CancellationToken cancellationToken = default);
}

public sealed class DiscordWebhookClient(
    IHttpClientFactory httpClientFactory,
    IDiscordWebhookPayloadValidator payloadValidator,
    ILogger<DiscordWebhookClient> logger) : IDiscordWebhookClient
{
    public async Task<bool> PostAsync(string webhookUrl, string renderedPayloadJson, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            return false;
        }

        try
        {
            payloadValidator.ValidateRendered(renderedPayloadJson);
        }
        catch (DiscordWebhookTemplateValidationException ex)
        {
            logger.LogWarning("Discord payload validation failed: {Message}", ex.Message);
            return false;
        }

        try
        {
            var client = httpClientFactory.CreateClient(nameof(DiscordWebhookClient));
            using var content = new StringContent(renderedPayloadJson, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(webhookUrl, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Discord webhook returned {StatusCode}", response.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send Discord webhook");
            return false;
        }
    }
}
