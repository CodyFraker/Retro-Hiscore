using System.Text.Json;

namespace RetroHiscore.Api.Features.Notifications;

public interface IDiscordWebhookPayloadValidator
{
    void ValidateTemplate(string payloadTemplateJson, IReadOnlySet<string> allowedTokenCodes);
    void ValidateRendered(string renderedPayloadJson);
}

public sealed class DiscordWebhookPayloadValidator : IDiscordWebhookPayloadValidator
{
    private const int MaxContentLength = 2000;
    private const int MaxEmbedTitle = 256;
    private const int MaxEmbedDescription = 4096;
    private const int MaxEmbeds = 10;

    public void ValidateTemplate(string payloadTemplateJson, IReadOnlySet<string> allowedTokenCodes)
    {
        if (string.IsNullOrWhiteSpace(payloadTemplateJson))
        {
            throw new DiscordWebhookTemplateValidationException("Payload template is required.");
        }

        try
        {
            using var doc = JsonDocument.Parse(payloadTemplateJson);
            if (!doc.RootElement.TryGetProperty("embeds", out var embeds)
                || embeds.ValueKind != JsonValueKind.Array
                || embeds.GetArrayLength() < 1)
            {
                throw new DiscordWebhookTemplateValidationException("Payload must include at least one embed.");
            }
        }
        catch (JsonException ex)
        {
            throw new DiscordWebhookTemplateValidationException($"Invalid JSON: {ex.Message}");
        }

        var renderer = new DiscordWebhookTemplateRenderer();
        var unknown = renderer.FindUnknownTokens(payloadTemplateJson, allowedTokenCodes);
        if (unknown.Count > 0)
        {
            throw new DiscordWebhookTemplateValidationException(
                $"Unknown tokens: {string.Join(", ", unknown.Select(c => "{{" + c + "}}"))}.");
        }
    }

    public void ValidateRendered(string renderedPayloadJson)
    {
        using var doc = JsonDocument.Parse(renderedPayloadJson);
        var root = doc.RootElement;

        if (root.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
        {
            var text = content.GetString() ?? "";
            if (text.Length > MaxContentLength)
            {
                throw new DiscordWebhookTemplateValidationException(
                    $"Content exceeds {MaxContentLength} characters.");
            }
        }

        if (!root.TryGetProperty("embeds", out var embeds) || embeds.ValueKind != JsonValueKind.Array)
        {
            throw new DiscordWebhookTemplateValidationException("Rendered payload must include embeds.");
        }

        if (embeds.GetArrayLength() > MaxEmbeds)
        {
            throw new DiscordWebhookTemplateValidationException($"At most {MaxEmbeds} embeds are allowed.");
        }

        foreach (var embed in embeds.EnumerateArray())
        {
            if (embed.TryGetProperty("title", out var title) && title.GetString()?.Length > MaxEmbedTitle)
            {
                throw new DiscordWebhookTemplateValidationException($"Embed title exceeds {MaxEmbedTitle} characters.");
            }

            if (embed.TryGetProperty("description", out var description)
                && description.GetString()?.Length > MaxEmbedDescription)
            {
                throw new DiscordWebhookTemplateValidationException(
                    $"Embed description exceeds {MaxEmbedDescription} characters.");
            }
        }
    }
}
