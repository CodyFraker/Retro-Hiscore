using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
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
            ILeaderboardSyncService leaderboardSync,
            IOptions<RaOptions> raOptions,
            CancellationToken ct) =>
        {
            var (game, error) = await AdminGameTracking.RefreshGameAsync(
                raGameId,
                db,
                gameMetadataSync,
                leaderboardSync,
                ct);
            if (error is not null)
            {
                return error;
            }

            var dto = await AdminGameMapper.ToDtoAsync(db, game!, raOptions, ct);
            return Results.Ok(dto);
        })
        .WithName("PostAdminGameRefresh")
        .WithTags("Admin")
        .WithSummary("Refreshes RetroAchievements metadata and friend leaderboard scores for one tracked game.")
        .RequireAdmin();
}
