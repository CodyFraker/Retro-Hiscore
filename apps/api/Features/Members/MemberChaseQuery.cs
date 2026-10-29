using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class MemberChaseQuery
{
    public static async Task<IReadOnlyList<MemberChaseItemDto>> LoadAsync(
        AppDbContext db,
        Guid memberId,
        CancellationToken cancellationToken)
    {
        var myEntries = await db.LeaderboardEntries
            .AsNoTracking()
            .Include(e => e.Leaderboard)
            .ThenInclude(l => l.Game)
            .Where(e => e.MemberId == memberId && e.FriendRank == 2)
            .ToListAsync(cancellationToken);

        if (myEntries.Count == 0)
        {
            return [];
        }

        var boardIds = myEntries.Select(e => e.LeaderboardId).Distinct().ToList();
        var leaders = await db.LeaderboardEntries
            .AsNoTracking()
            .Include(e => e.Member)
            .Where(e => boardIds.Contains(e.LeaderboardId) && e.FriendRank == 1)
            .ToDictionaryAsync(e => e.LeaderboardId, cancellationToken);

        var items = new List<MemberChaseItemDto>();

        foreach (var mine in myEntries)
        {
            if (!leaders.TryGetValue(mine.LeaderboardId, out var leader))
            {
                continue;
            }

            var board = mine.Leaderboard;
            var scoreGap = board.RankAsc
                ? mine.Score - leader.Score
                : leader.Score - mine.Score;

            items.Add(new MemberChaseItemDto(
                board.Game.RaGameId,
                board.Game.Title,
                board.RaLeaderboardId,
                board.Title,
                board.RankAsc,
                leader.Member.RaUsername ?? "",
                MemberAuthHelper.DisplayLabel(leader.Member),
                leader.FormattedScore,
                mine.FormattedScore,
                scoreGap,
                leader.ScoreUpdatedAt));
        }

        return items
            .OrderByDescending(i => i.LeaderScoreUpdatedAt ?? DateTimeOffset.MinValue)
            .ThenBy(i => i.GameTitle)
            .ThenBy(i => i.LeaderboardTitle)
            .ToList();
    }
}

public sealed record MemberChaseItemDto(
    int RaGameId,
    string GameTitle,
    long RaLeaderboardId,
    string LeaderboardTitle,
    bool RankAsc,
    string LeaderRaUsername,
    string LeaderDisplayName,
    string LeaderFormattedScore,
    string MyFormattedScore,
    long ScoreGap,
    DateTimeOffset? LeaderScoreUpdatedAt);

public sealed record MemberChaseResponse(IReadOnlyList<MemberChaseItemDto> Items);
