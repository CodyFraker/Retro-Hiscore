namespace RetroHiscore.Api.Domain;

public sealed class DiscordWebhookConfig
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public bool Enabled { get; set; }
    public string WebhookUrlProtected { get; set; } = "";
    public int DigestIntervalMinutes { get; set; }
    public string PayloadTemplateJson { get; set; } = "{}";
    public int[]? AllowedRaGameIds { get; set; }
    public DateTimeOffset? LastDispatchedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<DiscordWebhookEventSubscription> EventSubscriptions { get; set; } =
        new List<DiscordWebhookEventSubscription>();
}
