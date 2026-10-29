namespace RetroHiscore.Api.Domain;

public class SeasonBoardTake
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public Guid LeaderboardId { get; set; }
    public Leaderboard Leaderboard { get; set; } = null!;
    public DateTimeOffset TakenAt { get; set; }
}
