namespace RetroHiscore.Api.Domain;

public class GameTrackQueue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int RaGameId { get; set; }
    public GameTrackQueueStatus Status { get; set; } = GameTrackQueueStatus.Pending;
    public required string Title { get; set; }
    public string? ConsoleName { get; set; }
    public DateTimeOffset EnqueuedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public Guid? ResolvedByMemberId { get; set; }
    public Member? ResolvedByMember { get; set; }
    public string? FailureMessage { get; set; }
}
