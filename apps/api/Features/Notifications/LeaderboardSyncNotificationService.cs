using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Dashboard;

namespace RetroHiscore.Api.Features.Notifications;

public interface ILeaderboardSyncNotificationService
{
    Task EnqueueForGameAsync(Guid gameId, DateTimeOffset currentSyncedAt, CancellationToken cancellationToken = default);
}

public sealed class LeaderboardSyncNotificationService(
    AppDbContext db,
    INotificationOutboxWriter outboxWriter) : ILeaderboardSyncNotificationService
{
    public async Task EnqueueForGameAsync(
        Guid gameId,
        DateTimeOffset currentSyncedAt,
        CancellationToken cancellationToken = default)
    {
        var currentSync = currentSyncedAt;
        var leaderboardIds = await db.Leaderboards
            .AsNoTracking()
            .Where(l => l.GameId == gameId)
            .Select(l => l.Id)
            .ToListAsync(cancellationToken);

        if (leaderboardIds.Count == 0)
        {
            return;
        }

        var previous = await db.LeaderboardEntrySnapshots
            .AsNoTracking()
            .Where(s => leaderboardIds.Contains(s.LeaderboardId) && s.SyncedAt < currentSync)
            .Select(s => s.SyncedAt)
            .Distinct()
            .OrderByDescending(s => s)
            .FirstOrDefaultAsync(cancellationToken);

        if (previous == default)
        {
            return;
        }

        var previousSync = previous;

        var snapshots = await db.LeaderboardEntrySnapshots
            .Include(s => s.Member)
            .Include(s => s.Leaderboard)
            .ThenInclude(l => l.Game)
            .Where(s => leaderboardIds.Contains(s.LeaderboardId)
                && (s.SyncedAt == currentSync || s.SyncedAt == previousSync))
            .ToListAsync(cancellationToken);

        var populationByBoardAndSync = await db.LeaderboardPopulationSnapshots
            .AsNoTracking()
            .Where(p => leaderboardIds.Contains(p.LeaderboardId)
                && (p.SyncedAt == currentSync || p.SyncedAt == previousSync))
            .ToDictionaryAsync(p => (p.LeaderboardId, p.SyncedAt), p => p.EntryCount, cancellationToken);

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

            foreach (var memberSeries in series)
            {
                var hadPrevious = NotificationEventClassifier.HadPreviousSnapshot(
                    memberSeries.Points,
                    previousSync);
                var deltas = SnapshotDeltaCalculator.ComputeMemberDeltas([memberSeries]);
                var delta = deltas.FirstOrDefault();
                if (delta is null)
                {
                    continue;
                }

                var kind = NotificationEventClassifier.ClassifyLeaderboardDelta(delta, hadPrevious);
                if (kind is null)
                {
                    continue;
                }

                var currentSnapshot = boardGroup
                    .FirstOrDefault(s => s.MemberId == delta.MemberId && s.SyncedAt == currentSync);

                int? globalEntryCount = populationByBoardAndSync.TryGetValue(
                    (first.LeaderboardId, currentSync),
                    out var count)
                    ? count
                    : null;

                var payload = new LeaderboardNotificationPayload(
                    delta.MemberId,
                    currentSnapshot?.Member.RaUsername ?? "",
                    delta.DisplayName,
                    first.Leaderboard.Game.RaGameId,
                    first.Leaderboard.Game.Title,
                    first.Leaderboard.RaLeaderboardId,
                    first.Leaderboard.Title,
                    delta.ScoreDelta,
                    currentSnapshot?.FormattedScore,
                    delta.CurrentFriendRank,
                    delta.FriendRankDelta,
                    delta.CurrentGlobalRank,
                    globalEntryCount,
                    delta.GlobalRankDelta);

                await outboxWriter.EnqueueAsync(kind.Value, payload, cancellationToken);
            }
        }
    }
}
