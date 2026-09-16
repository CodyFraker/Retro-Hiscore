using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.GameOfTheWeek;

public interface IGameOfTheWeekVoteService
{
    Task<(GameOfTheWeekCurrentPollDto? Dto, IResult? Error)> CastOrUpdateVoteAsync(
        int raGameId,
        Guid memberId,
        CancellationToken ct);
}

public sealed class GameOfTheWeekVoteService(AppDbContext db) : IGameOfTheWeekVoteService
{
    public async Task<(GameOfTheWeekCurrentPollDto? Dto, IResult? Error)> CastOrUpdateVoteAsync(
        int raGameId,
        Guid memberId,
        CancellationToken ct)
    {
        var poll = await GameOfTheWeekPollQueries.GetBlockingPollForUpdateAsync(db, ct);
        if (poll is null)
        {
            return (null, Results.NotFound(new { message = "No active game-of-the-week poll." }));
        }

        var now = DateTimeOffset.UtcNow;
        var phase = GameOfTheWeekPollQueries.ResolvePhase(poll, now);
        if (phase != GameOfTheWeekPhase.Open)
        {
            return (null, Results.Conflict(new { message = "Voting is only allowed while the poll is open." }));
        }

        var onBallot = await db.GameOfTheWeekBallotEntries
            .AnyAsync(e => e.PollId == poll.Id && e.RaGameId == raGameId, ct);
        if (!onBallot)
        {
            return (null, Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(raGameId)] = ["That game is not on the ballot."]
            }));
        }

        var vote = await db.GameOfTheWeekVotes
            .FirstOrDefaultAsync(v => v.PollId == poll.Id && v.MemberId == memberId, ct);

        if (vote is null)
        {
            db.GameOfTheWeekVotes.Add(new GameOfTheWeekVote
            {
                PollId = poll.Id,
                MemberId = memberId,
                RaGameId = raGameId,
                CastAt = now
            });
        }
        else
        {
            vote.RaGameId = raGameId;
            vote.CastAt = now;
        }

        await db.SaveChangesAsync(ct);

        var dto = await GameOfTheWeekPollMapper.MapCurrentAsync(db, poll, now, memberId, ct);
        return (dto, null);
    }
}
