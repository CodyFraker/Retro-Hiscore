using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.GameOfTheWeek;

public static class GameOfTheWeekHistoryMapper
{
    public static async Task<GameOfTheWeekHistoryResponse> MapPageAsync(
        AppDbContext db,
        IReadOnlyList<GameOfTheWeekPoll> polls,
        int total,
        int offset,
        int limit,
        CancellationToken ct)
    {
        if (polls.Count == 0)
        {
            return new GameOfTheWeekHistoryResponse(total, offset, limit, []);
        }

        var pollIds = polls.Select(p => p.Id).ToList();
        var winnerIds = polls
            .Where(p => p.WinnerRaGameId is int id)
            .Select(p => p.WinnerRaGameId!.Value)
            .Distinct()
            .ToList();

        var ballotWinners = await db.GameOfTheWeekBallotEntries
            .AsNoTracking()
            .Where(e => pollIds.Contains(e.PollId) && winnerIds.Contains(e.RaGameId))
            .ToListAsync(ct);
        var ballotByPollAndGame = ballotWinners
            .GroupBy(e => (e.PollId, e.RaGameId))
            .ToDictionary(g => g.Key, g => g.First());

        var trackedSet = winnerIds.Count == 0
            ? new HashSet<int>()
            : (await db.Games
                .AsNoTracking()
                .Where(g => winnerIds.Contains(g.RaGameId))
                .Select(g => g.RaGameId)
                .ToListAsync(ct)).ToHashSet();

        var voteTotals = await db.GameOfTheWeekVotes
            .AsNoTracking()
            .Where(v => pollIds.Contains(v.PollId))
            .GroupBy(v => v.PollId)
            .Select(g => new { PollId = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var votesByPoll = voteTotals.ToDictionary(x => x.PollId, x => x.Count);

        var items = polls.Select(poll =>
        {
            GameOfTheWeekBallotEntry? winnerEntry = null;
            if (poll.WinnerRaGameId is int winnerId
                && ballotByPollAndGame.TryGetValue((poll.Id, winnerId), out var entry))
            {
                winnerEntry = entry;
            }

            var isTracked = poll.WinnerRaGameId is int raId && trackedSet.Contains(raId);

            return new GameOfTheWeekHistoryItemDto(
                poll.Id,
                poll.StartsAt,
                poll.EndsAt,
                poll.ClosedAt,
                poll.WinnerRaGameId,
                winnerEntry?.Title,
                winnerEntry?.ConsoleName,
                winnerEntry?.ImageIcon,
                isTracked,
                votesByPoll.GetValueOrDefault(poll.Id));
        }).ToList();

        return new GameOfTheWeekHistoryResponse(total, offset, limit, items);
    }
}
