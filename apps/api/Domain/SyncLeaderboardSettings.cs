namespace RetroHiscore.Api.Domain;

public sealed class SyncLeaderboardSettings
{
    public const int SingletonId = 1;

    public int Id { get; set; }
    public int HotIntervalMinutes { get; set; }
    public int ColdIntervalMinutes { get; set; }
    public int HotActivityWindowHours { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
