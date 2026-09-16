using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.GameOfTheWeek;

public interface IGameOfTheWeekCloseService
{
    Task CloseDuePollsAsync(CancellationToken ct);
}

public sealed class GameOfTheWeekCloseService(AppDbContext db, ILogger<GameOfTheWeekCloseService> logger) : IGameOfTheWeekCloseService
{
    public async Task CloseDuePollsAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var duePolls = await db.GameOfTheWeekPolls
            .Where(p => p.ClosedAt == null && p.EndsAt <= now)
            .ToListAsync(ct);

        foreach (var poll in duePolls)
        {
            await ClosePollAsync(poll, now, ct);
        }
    }

    private async Task ClosePollAsync(GameOfTheWeekPoll poll, DateTimeOffset closedAt, CancellationToken ct)
    {
        var entries = await db.GameOfTheWeekBallotEntries
            .AsNoTracking()
            .Where(e => e.PollId == poll.Id)
            .ToListAsync(ct);

        var voteCounts = await db.GameOfTheWeekVotes
            .AsNoTracking()
            .Where(v => v.PollId == poll.Id)
            .GroupBy(v => v.RaGameId)
            .Select(g => new { RaGameId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        int? winnerRaGameId = null;
        if (voteCounts.Count > 0)
        {
            var maxVotes = voteCounts.Max(x => x.Count);
            var tiedRaGameIds = voteCounts.Where(x => x.Count == maxVotes).Select(x => x.RaGameId).ToHashSet();
            winnerRaGameId = entries
                .Where(e => tiedRaGameIds.Contains(e.RaGameId))
                .OrderBy(e => e.SortOrder)
                .Select(e => (int?)e.RaGameId)
                .FirstOrDefault();
        }
        else if (entries.Count > 0)
        {
            winnerRaGameId = entries.OrderBy(e => e.SortOrder).First().RaGameId;
        }

        poll.ClosedAt = closedAt;
        poll.WinnerRaGameId = winnerRaGameId;

        if (winnerRaGameId is int winnerId)
        {
            var tracked = await db.Games.AnyAsync(g => g.RaGameId == winnerId, ct);
            poll.TrackingStatus = tracked
                ? GameOfTheWeekTrackingStatus.NotApplicable
                : GameOfTheWeekTrackingStatus.Pending;
        }
        else
        {
            poll.TrackingStatus = GameOfTheWeekTrackingStatus.NotApplicable;
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Closed game-of-the-week poll {PollId}; winner RaGameId={WinnerRaGameId}; tracking={TrackingStatus}",
            poll.Id,
            poll.WinnerRaGameId,
            poll.TrackingStatus);
    }
}
