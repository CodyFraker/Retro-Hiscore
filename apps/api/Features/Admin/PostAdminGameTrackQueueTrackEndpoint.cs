using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin;

public static class PostAdminGameTrackQueueTrackEndpoint
{
    public static RouteHandlerBuilder MapPostAdminGameTrackQueueTrack(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/admin/game-track-queue/{id:guid}/track", async (
            Guid id,
            AppDbContext db,
            IRaApiClient raApiClient,
            IRaApiKeyPool apiKeyPool,
            ILeaderboardSyncService leaderboardSync,
            ILeaderboardSyncJobEnqueuer jobEnqueuer,
            IConsoleIconSyncService consoleIconSync,
            IOptions<RaOptions> raOptions,
            ISyncSettingsStore syncSettingsStore,
            CancellationToken ct) =>
        {
            var item = await db.GameTrackQueues.FirstOrDefaultAsync(q => q.Id == id, ct);
            if (item is null)
            {
                return Results.NotFound();
            }

            if (item.Status != GameTrackQueueStatus.Pending)
            {
                return Results.Conflict(new { message = "Only pending queue items can be tracked." });
            }

            var (game, error) = await AdminGameTracking.AddGameAsync(
                item.RaGameId,
                db,
                raApiClient,
                apiKeyPool,
                leaderboardSync,
                jobEnqueuer,
                syncSettingsStore,
                consoleIconSync,
                raOptions,
                ct);
            if (error is not null)
            {
                return error;
            }

            var dto = await AdminGameMapper.ToDtoAsync(db, game!, raOptions, syncSettingsStore, ct);
            return Results.Ok(dto);
        })
        .WithName("PostAdminGameTrackQueueTrack")
        .WithTags("Admin")
        .WithSummary("Track a pending queue game: sync from RetroAchievements and mark queue items completed.")
        .RequireAdmin();
}
