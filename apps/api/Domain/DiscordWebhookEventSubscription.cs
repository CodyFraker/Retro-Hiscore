namespace RetroHiscore.Api.Domain;

public sealed class DiscordWebhookEventSubscription
{
    public Guid WebhookConfigId { get; set; }
    public DiscordWebhookConfig WebhookConfig { get; set; } = null!;
    public DiscordNotificationEventKind EventKind { get; set; }
}
