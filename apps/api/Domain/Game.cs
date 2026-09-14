namespace RetroHiscore.Api.Domain;

public class Game
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required int RaGameId { get; set; }
    public required string Title { get; set; }
    public int? ConsoleId { get; set; }
    public string? ConsoleName { get; set; }
    public string? ImageIcon { get; set; }
    public string? ImageTitle { get; set; }
    public string? ImageIngame { get; set; }
    public string? ImageBoxArt { get; set; }
    public string? Publisher { get; set; }
    public string? Developer { get; set; }
    public string? Genre { get; set; }
    public DateTimeOffset? ReleasedAt { get; set; }
    public DateTimeOffset? MetadataSyncedAt { get; set; }
    public ICollection<Leaderboard> Leaderboards { get; set; } = new List<Leaderboard>();
    public ICollection<GameSource> Sources { get; set; } = new List<GameSource>();
}
