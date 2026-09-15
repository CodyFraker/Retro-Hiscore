using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin;

public static class GetAdminGamesEndpoint
{
    public static RouteHandlerBuilder MapGetAdminGames(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/admin/games", async (
            AppDbContext db,
            IOptions<RaOptions> raOptions,
            ISyncSettingsStore syncSettingsStore,
            CancellationToken ct) =>
        {
            var mediaBaseUrl = raOptions.Value.MediaBaseUrl;

            var games = await db.Games
                .OrderBy(g => g.Title)
                .Select(g => new
                {
                    g.Id,
                    g.RaGameId,
                    g.Title,
                    g.ConsoleId,
                    g.ConsoleName,
                    g.ImageBoxArt,
                    g.ImageIcon,
                    g.ImageTitle,
                    g.ImageIngame,
                    g.MetadataSyncedAt,
                    g.LeaderboardScoresSyncedAt,
                    g.ForceColdLeaderboardSync,
                    LeaderboardCount = g.Leaderboards.Count,
                    SourceCount = g.Sources.Count,
                    ConsoleIconData = db.Consoles
                        .Where(c => c.RaConsoleId == g.ConsoleId)
                        .Select(c => c.IconData)
                        .FirstOrDefault(),
                    ConsoleIconContentType = db.Consoles
                        .Where(c => c.RaConsoleId == g.ConsoleId)
                        .Select(c => c.IconContentType)
                        .FirstOrDefault()
                })
                .ToListAsync(ct);

            var syncStatusByGameId = await GameLeaderboardSyncStatusQuery.GetForGamesAsync(
                db,
                syncSettingsStore,
                games.Select(g => new GameLeaderboardSyncStatusQuery.GameSyncInput(
                    g.Id,
                    g.RaGameId,
                    g.LeaderboardScoresSyncedAt,
                    g.ForceColdLeaderboardSync)).ToList(),
                ct);

            var dtos = games.Select(g => new AdminGameDto(
                    g.Id,
                    g.RaGameId,
                    g.Title,
                    g.ConsoleName,
                    ConsoleIconSyncService.ToDataUrl(g.ConsoleIconData, g.ConsoleIconContentType),
                    RaMediaUrl.ToAbsolute(g.ImageBoxArt, mediaBaseUrl),
                    RaMediaUrl.ToAbsolute(g.ImageIcon, mediaBaseUrl),
                    RaMediaUrl.ToAbsolute(g.ImageTitle, mediaBaseUrl),
                    RaMediaUrl.ToAbsolute(g.ImageIngame, mediaBaseUrl),
                    g.LeaderboardCount,
                    g.SourceCount,
                    g.MetadataSyncedAt,
                    g.ForceColdLeaderboardSync,
                    syncStatusByGameId[g.Id])).ToList();

            return Results.Ok(dtos);
        })
        .WithName("GetAdminGames")
        .WithTags("Admin")
        .WithSummary("Lists all tracked games with mirror metadata and hot/cold leaderboard sync schedule for administrators.")
        .RequireAdmin();
}
