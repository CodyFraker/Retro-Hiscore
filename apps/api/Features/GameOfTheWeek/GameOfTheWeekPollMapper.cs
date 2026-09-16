using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.GameOfTheWeek;

public static class GameOfTheWeekPollMapper
{
    public static async Task<GameOfTheWeekCurrentPollDto?> MapCurrentAsync(
        AppDbContext db,
        GameOfTheWeekPoll poll,
        DateTimeOffset now,
        Guid? memberId,
        CancellationToken ct)
    {
        var phase = GameOfTheWeekPollQueries.ResolvePhase(poll, now);

        var entries = await db.GameOfTheWeekBallotEntries
            .AsNoTracking()
            .Where(e => e.PollId == poll.Id)
            .OrderBy(e => e.SortOrder)
            .ToListAsync(ct);

        var voteCounts = await db.GameOfTheWeekVotes
            .AsNoTracking()
            .Where(v => v.PollId == poll.Id)
            .GroupBy(v => v.RaGameId)
            .Select(g => new { RaGameId = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var countByRaGameId = voteCounts.ToDictionary(x => x.RaGameId, x => x.Count);

        var trackedIds = await db.Games
            .AsNoTracking()
            .Where(g => entries.Select(e => e.RaGameId).Contains(g.RaGameId))
            .Select(g => g.RaGameId)
            .ToListAsync(ct);
        var trackedSet = trackedIds.ToHashSet();

        int? myVote = null;
        if (memberId is Guid mid)
        {
            myVote = await db.GameOfTheWeekVotes
                .AsNoTracking()
                .Where(v => v.PollId == poll.Id && v.MemberId == mid)
                .Select(v => (int?)v.RaGameId)
                .FirstOrDefaultAsync(ct);
        }

        var ballot = entries.Select(e => new GameOfTheWeekBallotItemDto(
            e.RaGameId,
            e.Title,
            e.ConsoleName,
            e.ImageIcon,
            e.SortOrder,
            trackedSet.Contains(e.RaGameId),
            countByRaGameId.GetValueOrDefault(e.RaGameId),
            e.AddedByMemberId)).ToList();

        return new GameOfTheWeekCurrentPollDto(
            poll.Id,
            phase,
            poll.StartsAt,
            poll.EndsAt,
            poll.ClosedAt,
            poll.WinnerRaGameId,
            poll.TrackingStatus,
            myVote,
            ballot,
            Math.Max(0, GameOfTheWeekConstants.MaxBallotSize - entries.Count));
    }
}
