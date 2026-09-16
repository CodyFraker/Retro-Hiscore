using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.GameOfTheWeek;

public interface IGameOfTheWeekPollService
{
    Task<(GameOfTheWeekCurrentPollDto? Dto, IResult? Error)> CreatePollAsync(
        PostGameOfTheWeekPollRequest request,
        Guid? createdByMemberId,
        CancellationToken ct);

    Task<GameOfTheWeekCurrentPollDto?> GetCurrentPollDtoAsync(Guid? memberId, CancellationToken ct);
}

public sealed class GameOfTheWeekPollService(
    AppDbContext db,
    IGameOfTheWeekRaGameResolver raGameResolver) : IGameOfTheWeekPollService
{
    public async Task<(GameOfTheWeekCurrentPollDto? Dto, IResult? Error)> CreatePollAsync(
        PostGameOfTheWeekPollRequest request,
        Guid? createdByMemberId,
        CancellationToken ct)
    {
        if (request.EndsAt <= request.StartsAt)
        {
            return (null, Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.EndsAt)] = ["endsAt must be after startsAt."]
            }));
        }

        var raGameIds = request.RaGameIds?.Distinct().ToList() ?? [];
        if (raGameIds.Count < GameOfTheWeekConstants.MinAdminSeedCount)
        {
            return (null, Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.RaGameIds)] = [$"At least {GameOfTheWeekConstants.MinAdminSeedCount} games are required."]
            }));
        }

        if (raGameIds.Count > GameOfTheWeekConstants.MaxBallotSize)
        {
            return (null, Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.RaGameIds)] = [$"At most {GameOfTheWeekConstants.MaxBallotSize} games are allowed on the ballot."]
            }));
        }

        var blocking = await GameOfTheWeekPollQueries.GetBlockingPollAsync(db, ct);
        if (blocking is not null)
        {
            return (null, Results.Conflict(new { message = "Another game-of-the-week poll is still active or awaiting tracking." }));
        }

        var snapshots = new List<ResolvedRaGameSnapshot>();
        foreach (var raGameId in raGameIds)
        {
            var (snapshot, error) = await raGameResolver.ResolveAsync(raGameId, ct);
            if (error is not null)
            {
                return (null, error);
            }

            snapshots.Add(snapshot!);
        }

        var now = DateTimeOffset.UtcNow;
        var poll = new GameOfTheWeekPoll
        {
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            CreatedAt = now,
            CreatedByMemberId = createdByMemberId,
            TrackingStatus = GameOfTheWeekTrackingStatus.NotApplicable
        };

        db.GameOfTheWeekPolls.Add(poll);

        var sortOrder = 1;
        foreach (var snapshot in snapshots)
        {
            db.GameOfTheWeekBallotEntries.Add(new GameOfTheWeekBallotEntry
            {
                PollId = poll.Id,
                RaGameId = snapshot.RaGameId,
                Title = snapshot.Title,
                ConsoleName = snapshot.ConsoleName,
                ImageIcon = snapshot.ImageIcon,
                SortOrder = sortOrder++,
                AddedAt = now
            });
        }

        await db.SaveChangesAsync(ct);

        var dto = await GameOfTheWeekPollMapper.MapCurrentAsync(db, poll, now, createdByMemberId, ct);
        return (dto, null);
    }

    public async Task<GameOfTheWeekCurrentPollDto?> GetCurrentPollDtoAsync(Guid? memberId, CancellationToken ct)
    {
        var poll = await GameOfTheWeekPollQueries.GetBlockingPollAsync(db, ct);
        if (poll is null)
        {
            return null;
        }

        return await GameOfTheWeekPollMapper.MapCurrentAsync(db, poll, DateTimeOffset.UtcNow, memberId, ct);
    }
}
