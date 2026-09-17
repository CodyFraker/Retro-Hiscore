using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Infrastructure;

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

        var voteRows = await db.GameOfTheWeekVotes
            .AsNoTracking()
            .Where(v => v.PollId == poll.Id)
            .Include(v => v.Member)
            .OrderBy(v => v.CastAt)
            .ToListAsync(ct);

        var countByRaGameId = voteRows
            .GroupBy(v => v.RaGameId)
            .ToDictionary(g => g.Key, g => g.Count());

        var votersByRaGameId = voteRows
            .GroupBy(v => v.RaGameId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<GameOfTheWeekVoteCastDto>)g
                    .Select(v => new GameOfTheWeekVoteCastDto(
                        v.MemberId,
                        MemberAuthHelper.DisplayLabel(v.Member),
                        v.CastAt))
                    .ToList());

        var nominatorIds = entries
            .Where(e => e.AddedByMemberId is Guid)
            .Select(e => e.AddedByMemberId!.Value)
            .Distinct()
            .ToList();

        var nominatorLabels = nominatorIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.Members
                .AsNoTracking()
                .Where(m => nominatorIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, MemberAuthHelper.DisplayLabel, ct);

        var trackedIds = await db.Games
            .AsNoTracking()
            .Where(g => entries.Select(e => e.RaGameId).Contains(g.RaGameId))
            .Select(g => g.RaGameId)
            .ToListAsync(ct);
        var trackedSet = trackedIds.ToHashSet();

        int? myVote = null;
        if (memberId is Guid mid)
        {
            myVote = voteRows
                .Where(v => v.MemberId == mid)
                .Select(v => (int?)v.RaGameId)
                .FirstOrDefault();
        }

        var ballot = entries.Select(e =>
        {
            var addedByDisplayName = e.AddedByMemberId is Guid nominatorId
                && nominatorLabels.TryGetValue(nominatorId, out var label)
                ? label
                : null;

            return new GameOfTheWeekBallotItemDto(
                e.RaGameId,
                e.Title,
                e.ConsoleName,
                e.ImageIcon,
                e.SortOrder,
                trackedSet.Contains(e.RaGameId),
                countByRaGameId.GetValueOrDefault(e.RaGameId),
                e.AddedByMemberId,
                addedByDisplayName,
                votersByRaGameId.GetValueOrDefault(e.RaGameId) ?? []);
        }).ToList();

        var eligibleVoterCount = await db.Members.CountAsync(m => m.RaUsername != null, ct);
        var votesCastCount = voteRows.Count;
        var allEligibleVotesCast = eligibleVoterCount > 0 && votesCastCount >= eligibleVoterCount;

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
            Math.Max(0, GameOfTheWeekConstants.MaxBallotSize - entries.Count),
            eligibleVoterCount,
            votesCastCount,
            allEligibleVotesCast);
    }
}
