namespace RetroHiscore.Api.Domain;

public class GameOfTheWeekBallotEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PollId { get; set; }
    public GameOfTheWeekPoll Poll { get; set; } = null!;
    public int RaGameId { get; set; }
    public required string Title { get; set; }
    public string? ConsoleName { get; set; }
    public string? ImageIcon { get; set; }
    public int SortOrder { get; set; }
    public Guid? AddedByMemberId { get; set; }
    public Member? AddedByMember { get; set; }
    public DateTimeOffset AddedAt { get; set; }
}
