namespace RetroHiscore.Api.Domain;

public sealed class NotificationOutboxDelivery
{
    public Guid OutboxId { get; set; }
    public NotificationOutbox Outbox { get; set; } = null!;
    public Guid WebhookConfigId { get; set; }
    public DiscordWebhookConfig WebhookConfig { get; set; } = null!;
    public DateTimeOffset? DispatchedAt { get; set; }
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
}
