namespace RetroHiscore.Api.Domain;

public sealed class NotificationOutbox
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DiscordNotificationEventKind EventKind { get; set; }
    public string PayloadJson { get; set; } = "";
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset? ReadyAt { get; set; }
    public Guid? SourceSyncRunId { get; set; }
    public SyncRun? SourceSyncRun { get; set; }
    public DateTimeOffset? DispatchedAt { get; set; }
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
}
