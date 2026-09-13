namespace RetroHiscore.Api.Domain;

public class Leaderboard
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required long RaLeaderboardId { get; set; }
    public Guid GameId { get; set; }
    public Game Game { get; set; } = null!;
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? Format { get; set; }
    public bool RankAsc { get; set; }
    public ICollection<LeaderboardEntry> Entries { get; set; } = new List<LeaderboardEntry>();
    public ICollection<LeaderboardEntrySnapshot> Snapshots { get; set; } = new List<LeaderboardEntrySnapshot>();
}
