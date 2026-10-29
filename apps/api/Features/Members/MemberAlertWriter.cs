using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Dashboard;
using RetroHiscore.Api.Features.Notifications;

namespace RetroHiscore.Api.Features.Members;

public interface IMemberAlertWriter
{
    Task WriteLeaderboardAlertsAsync(
        Guid leaderboardId,
        IReadOnlyList<SnapshotDeltaCalculator.MemberSnapshotSeries> series,
        DateTimeOffset previousSync,
        DateTimeOffset currentSync,
        Leaderboard board,
        CancellationToken cancellationToken = default);

    Task WriteFriendAchievementAlertsAsync(
        Member earner,
        int raGameId,
        string gameTitle,
        string achievementTitle,
        int achievementPoints,
        bool hardcore,
        DateTimeOffset earnedAt,
        CancellationToken cancellationToken = default);
}

public sealed class MemberAlertWriter(AppDbContext db) : IMemberAlertWriter
{
    public async Task WriteLeaderboardAlertsAsync(
        Guid leaderboardId,
        IReadOnlyList<SnapshotDeltaCalculator.MemberSnapshotSeries> series,
        DateTimeOffset previousSync,
        DateTimeOffset currentSync,
        Leaderboard board,
        CancellationToken cancellationToken = default)
    {
        var deltas = series
            .Select(memberSeries =>
            {
                var hadPrevious = NotificationEventClassifier.HadPreviousSnapshot(
                    memberSeries.Points,
                    previousSync);
                var computed = SnapshotDeltaCalculator.ComputeMemberDeltas([memberSeries]).FirstOrDefault();
                return (memberSeries, hadPrevious, computed);
            })
            .Where(x => x.computed is not null)
            .Select(x => (x.memberSeries, x.hadPrevious, delta: x.computed!))
            .ToList();

        foreach (var (_, _, delta) in deltas)
        {
            if (delta.CurrentFriendRank == 1
                && delta.FriendRankDelta is > 0
                && delta.PreviousFriendRank is not 1)
            {
                db.MemberAlerts.Add(new MemberAlert
                {
                    RecipientMemberId = delta.MemberId,
                    Kind = MemberAlertKind.YouTookBoard,
                    ActorMemberId = delta.MemberId,
                    RaGameId = board.Game.RaGameId,
                    GameTitle = board.Game.Title,
                    RaLeaderboardId = board.RaLeaderboardId,
                    LeaderboardTitle = board.Title,
                    OccurredAt = currentSync
                });

                db.SeasonBoardTakes.Add(new SeasonBoardTake
                {
                    MemberId = delta.MemberId,
                    LeaderboardId = leaderboardId,
                    TakenAt = currentSync
                });
            }
        }

        foreach (var (_, _, delta) in deltas)
        {
            if (delta.FriendRankDelta is not < 0
                || delta.CurrentFriendRank is null)
            {
                continue;
            }

            var passer = deltas
                .Where(x => x.delta.FriendRankDelta is > 0
                    && x.delta.CurrentFriendRank is not null
                    && x.delta.CurrentFriendRank < delta.CurrentFriendRank)
                .OrderBy(x => x.delta.CurrentFriendRank)
                .ThenByDescending(x => x.delta.FriendRankDelta)
                .Select(x => x.delta)
                .FirstOrDefault();

            if (passer is null)
            {
                continue;
            }

            db.MemberAlerts.Add(new MemberAlert
            {
                RecipientMemberId = delta.MemberId,
                Kind = MemberAlertKind.YouWerePassed,
                ActorMemberId = passer.MemberId,
                RaGameId = board.Game.RaGameId,
                GameTitle = board.Game.Title,
                RaLeaderboardId = board.RaLeaderboardId,
                LeaderboardTitle = board.Title,
                OccurredAt = currentSync
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task WriteFriendAchievementAlertsAsync(
        Member earner,
        int raGameId,
        string gameTitle,
        string achievementTitle,
        int achievementPoints,
        bool hardcore,
        DateTimeOffset earnedAt,
        CancellationToken cancellationToken = default)
    {
        var game = await db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.RaGameId == raGameId, cancellationToken);
        if (game is null)
        {
            return;
        }

        var friendIds = await db.LeaderboardEntries
            .AsNoTracking()
            .Where(e => e.MemberId != earner.Id
                && e.Leaderboard.GameId == game.Id)
            .Select(e => e.MemberId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var friendId in friendIds)
        {
            db.MemberAlerts.Add(new MemberAlert
            {
                RecipientMemberId = friendId,
                Kind = MemberAlertKind.FriendAchievement,
                ActorMemberId = earner.Id,
                RaGameId = raGameId,
                GameTitle = gameTitle,
                AchievementTitle = achievementTitle,
                AchievementPoints = achievementPoints,
                Hardcore = hardcore,
                OccurredAt = earnedAt
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
