namespace RetroHiscore.Api.Features.Notifications;

public static class DiscordWebhookUrlValidator
{
    private static readonly string[] AllowedHosts =
    [
        "discord.com",
        "discordapp.com",
    ];

    public static void Validate(string? webhookUrl)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            throw new DiscordWebhookValidationException("Webhook URL is required.");
        }

        if (!Uri.TryCreate(webhookUrl.Trim(), UriKind.Absolute, out var uri))
        {
            throw new DiscordWebhookValidationException("Webhook URL must be a valid absolute URL.");
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new DiscordWebhookValidationException("Webhook URL must use HTTPS.");
        }

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            throw new DiscordWebhookValidationException("Webhook URL must not include user credentials.");
        }

        if (!AllowedHosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase))
        {
            throw new DiscordWebhookValidationException("Webhook URL must be a Discord webhook URL.");
        }

        var path = uri.AbsolutePath;
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (!path.StartsWith("/api/webhooks/", StringComparison.OrdinalIgnoreCase)
            || segments.Length < 4)
        {
            throw new DiscordWebhookValidationException("Webhook URL must target /api/webhooks/{id}/{token}.");
        }
    }
}
