namespace RetroHiscore.Api.Domain;

public class LeaderboardPopulationSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LeaderboardId { get; set; }
    public Leaderboard Leaderboard { get; set; } = null!;
    public int EntryCount { get; set; }
    public DateTimeOffset SyncedAt { get; set; }
}
