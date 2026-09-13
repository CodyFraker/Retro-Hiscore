namespace RetroHiscore.Api.Domain;

public enum SyncTrigger
{
    Scheduled = 0,
    Manual = 1
}

public enum SyncRunStatus
{
    Running = 0,
    Succeeded = 1,
    Failed = 2,
    PartialSuccess = 3
}

public enum SyncKind
{
    LeaderboardScores = 0,
    GameMetadata = 1,
    ConsoleIcons = 2
}

public class SyncRun
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public SyncKind Kind { get; set; } = SyncKind.LeaderboardScores;
    public SyncTrigger Trigger { get; set; }
    public SyncRunStatus Status { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public string? Error { get; set; }
}
