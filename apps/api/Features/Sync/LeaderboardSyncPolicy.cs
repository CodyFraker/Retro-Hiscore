namespace RetroHiscore.Api.Features.Sync;

public sealed record LeaderboardSyncPolicy(
    int HotIntervalMinutes,
    int ColdIntervalMinutes,
    int HotActivityWindowHours)
{
    public static LeaderboardSyncPolicy Clamp(
        int hotIntervalMinutes,
        int coldIntervalMinutes,
        int hotActivityWindowHours)
        => new(
            Math.Clamp(hotIntervalMinutes, 1, 60 * 24),
            Math.Clamp(coldIntervalMinutes, 1, 60 * 24 * 7),
            Math.Max(1, hotActivityWindowHours));
}
