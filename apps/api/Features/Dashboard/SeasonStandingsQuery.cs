using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Dashboard;

public interface ISeasonBoardTakeBackfillService
{
    Task EnsureBackfilledAsync(CancellationToken cancellationToken = default);
}

public sealed class SeasonBoardTakeBackfillService(AppDbContext db) : ISeasonBoardTakeBackfillService
{
    public async Task EnsureBackfilledAsync(CancellationToken cancellationToken = default)
    {
        if (await db.SeasonBoardTakes.AnyAsync(cancellationToken))
        {
            return;
        }

        if (!await db.LeaderboardEntrySnapshots.AnyAsync(cancellationToken))
        {
            return;
        }

        var boardIds = await db.Leaderboards.AsNoTracking().Select(l => l.Id).ToListAsync(cancellationToken);

        foreach (var boardId in boardIds)
        {
            var snapshots = await db.LeaderboardEntrySnapshots
                .AsNoTracking()
                .Where(s => s.LeaderboardId == boardId)
                .OrderBy(s => s.SyncedAt)
                .ThenBy(s => s.MemberId)
                .ToListAsync(cancellationToken);

            var syncTimes = snapshots.Select(s => s.SyncedAt).Distinct().OrderBy(t => t).ToList();
            if (syncTimes.Count < 2)
            {
                continue;
            }

            var previousRanks = new Dictionary<Guid, int?>();

            foreach (var syncTime in syncTimes)
            {
                var atSync = snapshots.Where(s => s.SyncedAt == syncTime).ToList();
                var isFirstSync = syncTime == syncTimes[0];

                foreach (var row in atSync)
                {
                    if (!isFirstSync)
                    {
                        previousRanks.TryGetValue(row.MemberId, out var priorRank);
                        if (row.FriendRank == 1 && priorRank is not 1)
                        {
                            db.SeasonBoardTakes.Add(new SeasonBoardTake
                            {
                                MemberId = row.MemberId,
                                LeaderboardId = boardId,
                                TakenAt = syncTime
                            });
                        }
                    }
                }

                foreach (var row in atSync)
                {
                    previousRanks[row.MemberId] = row.FriendRank;
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}

public static class SeasonStandingsQuery
{
    public static async Task<SeasonStandingsResponse> LoadAsync(
        AppDbContext db,
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var start = new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddMonths(1);

        var takesInMonth = await db.SeasonBoardTakes
            .AsNoTracking()
            .Where(t => t.TakenAt >= start && t.TakenAt < end)
            .ToListAsync(cancellationToken);

        var memberIds = takesInMonth.Select(t => t.MemberId).Distinct().ToList();
        var members = await db.Members
            .AsNoTracking()
            .Where(m => memberIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, cancellationToken);

        var rows = takesInMonth
            .GroupBy(t => t.MemberId)
            .Select(g =>
            {
                var member = members[g.Key];
                return new SeasonStandingsRowDto(
                    g.Key,
                    member.RaUsername ?? "",
                    MemberAuthHelper.DisplayLabel(member),
                    member.AvatarUrl,
                    g.Count());
            })
            .OrderByDescending(r => r.BoardsTaken)
            .ThenBy(r => r.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var months = await db.SeasonBoardTakes
            .AsNoTracking()
            .Select(t => new { t.TakenAt.Year, t.TakenAt.Month })
            .Distinct()
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .Select(x => new SeasonMonthDto(x.Year, x.Month))
            .ToListAsync(cancellationToken);

        return new SeasonStandingsResponse(year, month, rows, months);
    }
}

public sealed record SeasonMonthDto(int Year, int Month);

public sealed record SeasonStandingsRowDto(
    Guid MemberId,
    string RaUsername,
    string DisplayName,
    string? AvatarUrl,
    int BoardsTaken);

public sealed record SeasonStandingsResponse(
    int Year,
    int Month,
    IReadOnlyList<SeasonStandingsRowDto> Rows,
    IReadOnlyList<SeasonMonthDto> AvailableMonths);
