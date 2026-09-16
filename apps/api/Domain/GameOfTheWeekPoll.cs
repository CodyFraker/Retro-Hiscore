namespace RetroHiscore.Api.Domain;

public class GameOfTheWeekPoll
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public int? WinnerRaGameId { get; set; }
    public GameOfTheWeekTrackingStatus TrackingStatus { get; set; } = GameOfTheWeekTrackingStatus.NotApplicable;
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedByMemberId { get; set; }
    public Member? CreatedByMember { get; set; }
    public ICollection<GameOfTheWeekBallotEntry> BallotEntries { get; set; } = new List<GameOfTheWeekBallotEntry>();
    public ICollection<GameOfTheWeekVote> Votes { get; set; } = new List<GameOfTheWeekVote>();
}
