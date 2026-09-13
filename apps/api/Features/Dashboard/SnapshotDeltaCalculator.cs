namespace RetroHiscore.Api.Features.Dashboard;

public static class SnapshotDeltaCalculator
{
    public sealed record SnapshotPoint(DateTimeOffset SyncedAt, long Score, int? FriendRank);

    public sealed record MemberSnapshotSeries(Guid MemberId, string DisplayName, IReadOnlyList<SnapshotPoint> Points);

    public sealed record MemberSnapshotDelta(
        Guid MemberId,
        string DisplayName,
        long? ScoreDelta,
        long? PreviousScore,
        long? CurrentScore,
        int? FriendRankDelta,
        int? PreviousFriendRank,
        int? CurrentFriendRank);

    public static IReadOnlyList<MemberSnapshotDelta> ComputeMemberDeltas(IReadOnlyList<MemberSnapshotSeries> series)
    {
        return series.Select(member =>
        {
            var points = member.Points.OrderBy(p => p.SyncedAt).ToList();
            if (points.Count < 2)
            {
                var last = points.LastOrDefault();
                return new MemberSnapshotDelta(
                    member.MemberId,
                    member.DisplayName,
                    null,
                    null,
                    last?.Score,
                    null,
                    null,
                    last?.FriendRank);
            }

            var previous = points[^2];
            var current = points[^1];
            int? friendRankDelta = previous.FriendRank is not null && current.FriendRank is not null
                ? previous.FriendRank - current.FriendRank
                : null;

            return new MemberSnapshotDelta(
                member.MemberId,
                member.DisplayName,
                current.Score - previous.Score,
                previous.Score,
                current.Score,
                friendRankDelta,
                previous.FriendRank,
                current.FriendRank);
        }).ToList();
    }

    public static IReadOnlyList<MemberSnapshotDelta> ComputeActionableDeltas(IReadOnlyList<MemberSnapshotSeries> series)
        => ComputeMemberDeltas(series)
            .Where(d =>
                (d.ScoreDelta is not null && d.ScoreDelta != 0) ||
                (d.FriendRankDelta is not null && d.FriendRankDelta != 0))
            .ToList();
}
