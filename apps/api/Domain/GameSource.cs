namespace RetroHiscore.Api.Domain;

public class GameSource
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GameId { get; set; }
    public Game Game { get; set; } = null!;
    public GameSourceType SourceType { get; set; }
    public required string Url { get; set; }
    public string? Label { get; set; }
    public int SortOrder { get; set; }
    public string? Note { get; set; }
}
