using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Dashboard;

namespace RetroHiscore.Api.Features.Notifications;

public static class NotificationEventClassifier
{
    public static DiscordNotificationEventKind? ClassifyLeaderboardDelta(
        SnapshotDeltaCalculator.MemberSnapshotDelta delta,
        bool hadPreviousSnapshot)
    {
        if (delta.FriendRankDelta is > 0)
        {
            return DiscordNotificationEventKind.LeaderboardFriendOvertake;
        }

        if (IsNewSubmission(delta, hadPreviousSnapshot))
        {
            return DiscordNotificationEventKind.LeaderboardNewSubmission;
        }

        return null;
    }

    private static bool IsNewSubmission(
        SnapshotDeltaCalculator.MemberSnapshotDelta delta,
        bool hadPreviousSnapshot)
    {
        if (!hadPreviousSnapshot)
        {
            return delta.CurrentScore is not null;
        }

        return delta.PreviousFriendRank is null && delta.CurrentFriendRank is not null;
    }

    public static bool HadPreviousSnapshot(IReadOnlyList<SnapshotDeltaCalculator.SnapshotPoint> points, DateTimeOffset previousSync)
        => points.Any(p => p.SyncedAt == previousSync);
}
