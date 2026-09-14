using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;

namespace RetroHiscore.Api.Features.Admin;

public static class AdminGameMapper
{
    public static async Task<AdminGameDto?> ToDtoAsync(
        AppDbContext db,
        Game game,
        IOptions<RaOptions> raOptions,
        CancellationToken ct)
    {
        var leaderboardCount = await db.Leaderboards.CountAsync(l => l.GameId == game.Id, ct);
        var sourceCount = await db.GameSources.CountAsync(s => s.GameId == game.Id, ct);
        var images = RaMediaUrl.FromGame(game, raOptions.Value.MediaBaseUrl);
        var console = game.ConsoleId is null
            ? null
            : await db.Consoles.AsNoTracking().FirstOrDefaultAsync(c => c.RaConsoleId == game.ConsoleId, ct);

        return new AdminGameDto(
            game.Id,
            game.RaGameId,
            game.Title,
            game.ConsoleName,
            ConsoleIconSyncService.ToDataUrl(console?.IconData, console?.IconContentType),
            images.ImageBoxArtUrl,
            images.ImageIconUrl,
            images.ImageTitleUrl,
            images.ImageIngameUrl,
            leaderboardCount,
            sourceCount,
            game.MetadataSyncedAt);
    }
}
