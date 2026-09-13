namespace RetroHiscore.Api.Domain;

public class LeaderboardEntrySnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LeaderboardId { get; set; }
    public Leaderboard Leaderboard { get; set; } = null!;
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public long Score { get; set; }
    public string FormattedScore { get; set; } = string.Empty;
    public int? GlobalRank { get; set; }
    public int? FriendRank { get; set; }
    public DateTimeOffset SyncedAt { get; set; }
}
