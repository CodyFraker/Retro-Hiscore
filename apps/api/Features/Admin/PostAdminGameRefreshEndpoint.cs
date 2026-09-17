using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin;

public static class PostAdminGameRefreshEndpoint
{
    public static RouteHandlerBuilder MapPostAdminGameRefresh(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/admin/games/{raGameId:int}/refresh", async (
            int raGameId,
            AppDbContext db,
            IGameMetadataSyncService gameMetadataSync,
            ILeaderboardSyncJobEnqueuer jobEnqueuer,
            IOptions<RaOptions> raOptions,
            ISyncSettingsStore syncSettingsStore,
            CancellationToken ct) =>
        {
            var (game, error) = await AdminGameTracking.RefreshGameAsync(
                raGameId,
                db,
                gameMetadataSync,
                ct);
            if (error is not null)
            {
                return error;
            }

            await LeaderboardSyncJobEnqueueExtensions.EnqueueGameShellAndEligibleMembersAsync(
                jobEnqueuer,
                db,
                syncSettingsStore,
                raGameId,
                SyncTrigger.Manual,
                cancellationToken: ct);

            var dto = await AdminGameMapper.ToDtoAsync(db, game!, raOptions, syncSettingsStore, ct);
            return Results.Ok(dto);
        })
        .WithName("PostAdminGameRefresh")
        .WithTags("Admin")
        .WithSummary("Refreshes RetroAchievements metadata and queues game-shell plus member-game leaderboard sync for members recently on this title.")
        .RequireAdmin();
}
