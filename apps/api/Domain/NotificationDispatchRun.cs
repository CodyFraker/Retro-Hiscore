namespace RetroHiscore.Api.Domain;

public sealed class NotificationDispatchRun
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public SyncRunStatus Status { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public string? Error { get; set; }
    public int EventsProcessed { get; set; }
    public int PostsSucceeded { get; set; }
    public int PostsFailed { get; set; }
}
