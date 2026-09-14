namespace RetroHiscore.Api.Domain;

public class MemberRecentGamePlay
{
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public int RaGameId { get; set; }
    public required string Title { get; set; }
    public int ConsoleId { get; set; }
    public string? ConsoleName { get; set; }
    public string? ImageIcon { get; set; }
    public string? ImageBoxArt { get; set; }
    public DateTimeOffset LastPlayedAt { get; set; }
    public int NumAchieved { get; set; }
    public int NumPossibleAchievements { get; set; }
    public DateTimeOffset SyncedAt { get; set; }
}
