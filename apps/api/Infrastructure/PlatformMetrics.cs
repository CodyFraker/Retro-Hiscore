using System.Diagnostics.Metrics;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Infrastructure;

public static class PlatformMetrics
{
    public const string MeterName = "RetroHiscore.Platform";

    private static readonly Meter Meter = new(MeterName);

    private static readonly Counter<long> HangfireJobCompleted = Meter.CreateCounter<long>(
        "hangfire.job.completed",
        description: "Hangfire background jobs completed");

    private static readonly Histogram<double> HangfireJobDurationMs = Meter.CreateHistogram<double>(
        "hangfire.job.duration",
        unit: "ms",
        description: "Hangfire background job duration");

    private static readonly Counter<long> SyncRunCompleted = Meter.CreateCounter<long>(
        "sync.run.completed",
        description: "Sync runs completed");

    private static readonly Histogram<double> SyncRunDurationMs = Meter.CreateHistogram<double>(
        "sync.run.duration",
        unit: "ms",
        description: "Sync run duration");

    private static readonly Counter<long> GameTrackRequestSubmitted = Meter.CreateCounter<long>(
        "game.track_request.submitted",
        description: "Member game track requests submitted");

    private static readonly Counter<long> MemberSelfSyncRequested = Meter.CreateCounter<long>(
        "member.self_sync.requested",
        description: "Member-initiated self sync requests");

    private static readonly Counter<long> SearchExecuted = Meter.CreateCounter<long>(
        "search.executed",
        description: "Search requests executed");

    private static long _leaderboardSyncInProgress;
    private static long _membersTotal;
    private static long _membersWithRaApiKey;
    private static long _gameTrackQueuePending;
    private static long _notificationOutboxUndispatched;

    private static readonly ObservableGauge<long> LeaderboardSyncInProgressGauge = Meter.CreateObservableGauge(
        "sync.leaderboard.in_progress",
        () => _leaderboardSyncInProgress,
        description: "Leaderboard score sync runs currently in progress");

    private static readonly ObservableGauge<long> MembersTotalGauge = Meter.CreateObservableGauge(
        "members.total",
        () => _membersTotal,
        description: "Members with a linked Discord account");

    private static readonly ObservableGauge<long> MembersWithRaApiKeyGauge = Meter.CreateObservableGauge(
        "members.with_ra_api_key",
        () => _membersWithRaApiKey,
        description: "Members with a stored RetroAchievements API key");

    private static readonly ObservableGauge<long> GameTrackQueuePendingGauge = Meter.CreateObservableGauge(
        "game.track_queue.pending",
        () => _gameTrackQueuePending,
        description: "Pending game track queue items");

    private static readonly ObservableGauge<long> NotificationOutboxUndispatchedGauge = Meter.CreateObservableGauge(
        "notification_outbox.undispatched",
        () => _notificationOutboxUndispatched,
        description: "Notification outbox rows not yet dispatched");

    public static void RecordHangfireJob(string jobName, string status, double durationMs)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("job_name", jobName),
            new("status", status)
        };
        HangfireJobCompleted.Add(1, tags);
        HangfireJobDurationMs.Record(durationMs, tags);
    }

    public static void RecordSyncRun(SyncRun run)
    {
        if (run.FinishedAt is null)
        {
            return;
        }

        var durationMs = (run.FinishedAt.Value - run.StartedAt).TotalMilliseconds;
        var tags = new KeyValuePair<string, object?>[]
        {
            new("kind", run.Kind.ToString()),
            new("status", run.Status.ToString()),
            new("trigger", run.Trigger.ToString())
        };
        SyncRunCompleted.Add(1, tags);
        SyncRunDurationMs.Record(durationMs, tags);
    }

    public static void RecordGameTrackRequestSubmitted() => GameTrackRequestSubmitted.Add(1);

    public static void RecordMemberSelfSyncRequested(string operation)
    {
        MemberSelfSyncRequested.Add(1, new KeyValuePair<string, object?>("operation", operation));
    }

    public static void RecordSearchExecuted() => SearchExecuted.Add(1);

    public static void SetLeaderboardSyncInProgress(long value) => _leaderboardSyncInProgress = value;

    public static void SetMembersTotal(long value) => _membersTotal = value;

    public static void SetMembersWithRaApiKey(long value) => _membersWithRaApiKey = value;

    public static void SetGameTrackQueuePending(long value) => _gameTrackQueuePending = value;

    public static void SetNotificationOutboxUndispatched(long value) => _notificationOutboxUndispatched = value;
}
