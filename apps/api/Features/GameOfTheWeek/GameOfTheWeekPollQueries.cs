using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.GameOfTheWeek;

public static class GameOfTheWeekPollQueries
{
    public static GameOfTheWeekPhase ResolvePhase(GameOfTheWeekPoll poll, DateTimeOffset now)
    {
        if (poll.ClosedAt is not null)
        {
            return GameOfTheWeekPhase.Closed;
        }

        if (now < poll.StartsAt)
        {
            return GameOfTheWeekPhase.Scheduled;
        }

        if (now < poll.EndsAt)
        {
            return GameOfTheWeekPhase.Open;
        }

        return GameOfTheWeekPhase.Closed;
    }

    public static async Task<GameOfTheWeekPoll?> GetBlockingPollAsync(AppDbContext db, CancellationToken ct)
    {
        return await db.GameOfTheWeekPolls
            .AsNoTracking()
            .Where(p =>
                p.ClosedAt == null
                || p.TrackingStatus == GameOfTheWeekTrackingStatus.Pending)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public static async Task<GameOfTheWeekPoll?> GetBlockingPollForUpdateAsync(AppDbContext db, CancellationToken ct)
    {
        return await db.GameOfTheWeekPolls
            .Where(p =>
                p.ClosedAt == null
                || p.TrackingStatus == GameOfTheWeekTrackingStatus.Pending)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }
}
