using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin;

public static class PostAdminGameEndpoint
{
    public static RouteHandlerBuilder MapPostAdminGame(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/admin/games", async (
            PostAdminGameRequest request,
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
            var (game, error) = await AdminGameTracking.AddGameAsync(
                request.RaGameId,
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
            return Results.Created($"/api/admin/games/{game!.RaGameId}", dto);
        })
        .WithName("PostAdminGame")
        .WithTags("Admin")
        .WithSummary("Track a new RetroAchievements game by id and sync its metadata and member scores.")
        .RequireAdmin();
}
