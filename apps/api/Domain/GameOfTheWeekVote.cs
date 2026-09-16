namespace RetroHiscore.Api.Domain;

public class GameOfTheWeekVote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PollId { get; set; }
    public GameOfTheWeekPoll Poll { get; set; } = null!;
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public int RaGameId { get; set; }
    public DateTimeOffset CastAt { get; set; }
}
