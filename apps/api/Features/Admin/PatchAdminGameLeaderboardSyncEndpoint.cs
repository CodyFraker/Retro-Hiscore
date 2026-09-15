using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin;

public static class PatchAdminGameLeaderboardSyncEndpoint
{
    public static RouteHandlerBuilder MapPatchAdminGameLeaderboardSync(this IEndpointRouteBuilder routes)
        => routes.MapPatch("/api/admin/games/{raGameId:int}/leaderboard-sync", async (
            int raGameId,
            PatchAdminGameLeaderboardSyncRequest request,
            AppDbContext db,
            IOptions<RaOptions> raOptions,
            ISyncSettingsStore syncSettingsStore,
            CancellationToken ct) =>
        {
            var game = await db.Games.FirstOrDefaultAsync(g => g.RaGameId == raGameId, ct);
            if (game is null)
            {
                return Results.NotFound();
            }

            game.ForceColdLeaderboardSync = request.ForceColdLeaderboardSync;
            await db.SaveChangesAsync(ct);

            var dto = await AdminGameMapper.ToDtoAsync(db, game, raOptions, syncSettingsStore, ct);
            return Results.Ok(dto);
        })
        .WithName("PatchAdminGameLeaderboardSync")
        .WithTags("Admin")
        .WithSummary("Pins a tracked game to the cold leaderboard sync schedule to reduce RetroAchievements API usage.")
        .RequireAdmin();
}
