using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.GameOfTheWeek;

public interface IGameOfTheWeekBallotService
{
    Task<(GameOfTheWeekCurrentPollDto? Dto, IResult? Error)> AddBallotGameAsync(
        int raGameId,
        Guid memberId,
        CancellationToken ct);
}

public sealed class GameOfTheWeekBallotService(
    AppDbContext db,
    IGameOfTheWeekRaGameResolver raGameResolver) : IGameOfTheWeekBallotService
{
    public async Task<(GameOfTheWeekCurrentPollDto? Dto, IResult? Error)> AddBallotGameAsync(
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
            return (null, Results.Conflict(new { message = "Ballot changes are only allowed while voting is open." }));
        }

        var existingOnBallot = await db.GameOfTheWeekBallotEntries
            .AnyAsync(e => e.PollId == poll.Id && e.RaGameId == raGameId, ct);
        if (existingOnBallot)
        {
            return (null, Results.Conflict(new { message = "That game is already on the ballot." }));
        }

        var count = await db.GameOfTheWeekBallotEntries.CountAsync(e => e.PollId == poll.Id, ct);
        if (count >= GameOfTheWeekConstants.MaxBallotSize)
        {
            return (null, Results.Conflict(new { message = "The ballot is full." }));
        }

        var (snapshot, resolveError) = await raGameResolver.ResolveAsync(raGameId, ct);
        if (resolveError is not null)
        {
            return (null, resolveError);
        }

        var nextSort = count + 1;
        db.GameOfTheWeekBallotEntries.Add(new GameOfTheWeekBallotEntry
        {
            PollId = poll.Id,
            RaGameId = snapshot!.RaGameId,
            Title = snapshot.Title,
            ConsoleName = snapshot.ConsoleName,
            ImageIcon = snapshot.ImageIcon,
            SortOrder = nextSort,
            AddedByMemberId = memberId,
            AddedAt = now
        });

        await db.SaveChangesAsync(ct);

        var dto = await GameOfTheWeekPollMapper.MapCurrentAsync(db, poll, now, memberId, ct);
        return (dto, null);
    }
}
