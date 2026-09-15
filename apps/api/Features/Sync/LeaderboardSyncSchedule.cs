namespace RetroHiscore.Api.Features.Sync;

public enum LeaderboardSyncTier
{
    Hot = 0,
    Cold = 1
}

public sealed record LeaderboardSyncScheduleResult(
    LeaderboardSyncTier Tier,
    bool IsDue,
    DateTimeOffset? NextDueAt);

public static class LeaderboardSyncSchedule
{
    public static LeaderboardSyncScheduleResult Evaluate(
        DateTimeOffset utcNow,
        DateTimeOffset? lastSyncedAt,
        DateTimeOffset? maxLastPlayedAt,
        LeaderboardSyncPolicy policy,
        bool forceColdLeaderboardSync = false)
    {
        var hotWindow = TimeSpan.FromHours(policy.HotActivityWindowHours);
        var hotInterval = TimeSpan.FromMinutes(policy.HotIntervalMinutes);
        var coldInterval = TimeSpan.FromMinutes(policy.ColdIntervalMinutes);

        var tier = forceColdLeaderboardSync
            ? LeaderboardSyncTier.Cold
            : maxLastPlayedAt is { } played && played >= utcNow - hotWindow
                ? LeaderboardSyncTier.Hot
                : LeaderboardSyncTier.Cold;

        var interval = tier == LeaderboardSyncTier.Hot ? hotInterval : coldInterval;

        if (lastSyncedAt is null)
        {
            return new LeaderboardSyncScheduleResult(tier, true, utcNow);
        }

        var nextDue = lastSyncedAt.Value + interval;
        var isDue = utcNow >= nextDue;
        return new LeaderboardSyncScheduleResult(tier, isDue, isDue ? utcNow : nextDue);
    }
}
