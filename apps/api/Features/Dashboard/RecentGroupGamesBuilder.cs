using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.Dashboard;

public static class RecentGroupGamesBuilder
{
    public static async Task<IReadOnlyList<RecentGroupGameDto>> BuildAsync(
        AppDbContext db,
        IOptions<SyncOptions> syncOptions,
        IOptions<RaOptions> raOptions,
        CancellationToken ct)
    {
        var limit = syncOptions.Value.DashboardRecentGamesLimit;
        var mediaBaseUrl = raOptions.Value.MediaBaseUrl;

        var plays = await db.MemberRecentGamePlays
            .Include(p => p.Member)
            .AsNoTracking()
            .ToListAsync(ct);

        if (plays.Count == 0)
        {
            return [];
        }

        var trackedSet = (await db.Games.AsNoTracking().Select(g => g.RaGameId).ToListAsync(ct)).ToHashSet();

        var consoleIds = plays.Select(p => p.ConsoleId).Distinct().ToList();
        var consoleRows = await db.Consoles
            .AsNoTracking()
            .Where(c => consoleIds.Contains(c.RaConsoleId))
            .Select(c => new { c.RaConsoleId, c.IconData, c.IconContentType })
            .ToListAsync(ct);
        var consoleIcons = consoleRows.ToDictionary(
            c => c.RaConsoleId,
            c => ConsoleIconSyncService.ToDataUrl(c.IconData, c.IconContentType));

        return plays
            .GroupBy(p => p.RaGameId)
            .Select(group =>
            {
                var latest = group.OrderByDescending(p => p.LastPlayedAt).First();
                var players = group
                    .Select(p =>
                    {
                        var member = p.Member;
                        return new RecentGroupGamePlayerDto(
                            member.Id,
                            member.RaUsername ?? string.Empty,
                            MemberAuthHelper.DisplayLabel(member),
                            member.AvatarUrl,
                            p.LastPlayedAt);
                    })
                    .OrderByDescending(p => p.LastPlayedAt)
                    .ThenBy(p => p.DisplayName, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return new RecentGroupGameDto(
                    latest.RaGameId,
                    latest.Title,
                    latest.ConsoleId,
                    latest.ConsoleName,
                    consoleIcons.GetValueOrDefault(latest.ConsoleId),
                    RaMediaUrl.ToAbsolute(latest.ImageBoxArt, mediaBaseUrl),
                    RaMediaUrl.ToAbsolute(latest.ImageIcon, mediaBaseUrl),
                    group.Max(p => p.LastPlayedAt),
                    trackedSet.Contains(latest.RaGameId),
                    players);
            })
            .OrderByDescending(g => g.LastPlayedAt)
            .ThenBy(g => g.Title, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .ToList();
    }
}
