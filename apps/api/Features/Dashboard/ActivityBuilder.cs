using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;

namespace RetroHiscore.Api.Features.Dashboard;

public static class ActivityBuilder
{
    public static async Task<IReadOnlyList<ActivityItemDto>> BuildAsync(AppDbContext db, CancellationToken ct)
    {
        var syncStamps = await db.LeaderboardEntrySnapshots
            .Select(s => s.SyncedAt)
            .Distinct()
            .OrderByDescending(s => s)
            .Take(2)
            .ToListAsync(ct);

        if (syncStamps.Count < 2)
        {
            return [];
        }

        var currentSync = syncStamps[0];
        var previousSync = syncStamps[1];

        var snapshots = await db.LeaderboardEntrySnapshots
            .Include(s => s.Member)
            .Include(s => s.Leaderboard)
            .ThenInclude(l => l.Game)
            .Where(s => s.SyncedAt == currentSync || s.SyncedAt == previousSync)
            .ToListAsync(ct);

        var populationByBoardAndSync = await db.LeaderboardPopulationSnapshots
            .Where(p => p.SyncedAt == currentSync || p.SyncedAt == previousSync)
            .ToDictionaryAsync(p => (p.LeaderboardId, p.SyncedAt), p => p.EntryCount, ct);

        var activity = new List<ActivityItemDto>();

        foreach (var boardGroup in snapshots.GroupBy(s => s.LeaderboardId))
        {
            var first = boardGroup.First();
            var series = boardGroup
                .GroupBy(s => s.MemberId)
                .Select(memberGroup =>
                {
                    var member = memberGroup.First().Member;
                    return new SnapshotDeltaCalculator.MemberSnapshotSeries(
                        memberGroup.Key,
                        member.DisplayName ?? member.RaUsername ?? string.Empty,
                        memberGroup.Select(s => new SnapshotDeltaCalculator.SnapshotPoint(
                            s.SyncedAt,
                            s.Score,
                            s.FriendRank,
                            s.GlobalRank)).ToList());
                })
                .ToList();

            foreach (var delta in SnapshotDeltaCalculator.ComputeActionableDeltas(series))
            {
                var currentSnapshot = boardGroup
                    .Where(s => s.MemberId == delta.MemberId && s.SyncedAt == currentSync)
                    .FirstOrDefault();

                int? globalEntryCount = populationByBoardAndSync.TryGetValue(
                    (first.LeaderboardId, currentSync),
                    out var count)
                    ? count
                    : null;

                activity.Add(new ActivityItemDto(
                    delta.MemberId,
                    currentSnapshot?.Member.RaUsername ?? "",
                    delta.DisplayName,
                    currentSnapshot?.Member.AvatarUrl,
                    first.Leaderboard.Game.RaGameId,
                    first.Leaderboard.Game.Title,
                    first.Leaderboard.RaLeaderboardId,
                    first.Leaderboard.Title,
                    delta.ScoreDelta,
                    delta.FriendRankDelta,
                    delta.CurrentGlobalRank,
                    globalEntryCount,
                    delta.GlobalRankDelta,
                    currentSnapshot?.FormattedScore));
            }
        }

        return activity
            .OrderByDescending(a => a.FriendRankDelta is not null && a.FriendRankDelta != 0 ? 1 : 0)
            .ThenByDescending(a => Math.Abs(a.ScoreDelta ?? 0))
            .ThenBy(a => a.GameTitle)
            .ThenBy(a => a.LeaderboardTitle)
            .ThenBy(a => a.DisplayName)
            .ToList();
    }
}
