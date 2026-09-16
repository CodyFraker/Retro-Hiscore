using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Sync;

public static class SyncGroupHealthEvaluator
{
    public const string StatusHealthy = "Healthy";
    public const string StatusDegraded = "Degraded";
    public const string StatusRateLimited = "RateLimited";

    public static bool IsRateLimitedError(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            return false;
        }

        return error.Contains("rate limit", StringComparison.OrdinalIgnoreCase)
            || error.Contains("RaApiRateLimited", StringComparison.OrdinalIgnoreCase)
            || error.Contains("429", StringComparison.Ordinal);
    }

    public static string DeriveGroupStatus(SyncRun? latestLeaderboard)
    {
        if (latestLeaderboard is null)
        {
            return StatusDegraded;
        }

        if (latestLeaderboard.Status is SyncRunStatus.Failed or SyncRunStatus.PartialSuccess
            && IsRateLimitedError(latestLeaderboard.Error))
        {
            return StatusRateLimited;
        }

        return latestLeaderboard.Status switch
        {
            SyncRunStatus.Succeeded => StatusHealthy,
            SyncRunStatus.Running => StatusHealthy,
            SyncRunStatus.PartialSuccess => StatusDegraded,
            SyncRunStatus.Failed => StatusDegraded,
            _ => StatusDegraded
        };
    }
}
