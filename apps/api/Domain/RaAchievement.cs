namespace RetroHiscore.Api.Domain;

public class RaAchievement
{
    public int RaAchievementId { get; set; }
    public int RaGameId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int Points { get; set; }
    public int TrueRatio { get; set; }
    public string? BadgeName { get; set; }
    public int DisplayOrder { get; set; }
    public string? Type { get; set; }
    public byte[]? BadgeData { get; set; }
    public string? BadgeContentType { get; set; }
    public DateTimeOffset? SyncedAt { get; set; }
}
