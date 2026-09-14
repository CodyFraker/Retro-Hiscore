using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin;

public static class DeleteAdminGameEndpoint
{
    public static RouteHandlerBuilder MapDeleteAdminGame(this IEndpointRouteBuilder routes)
        => routes.MapDelete("/api/admin/games/{raGameId:int}", async (
            int raGameId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var error = await AdminGameTracking.DeleteGameAsync(raGameId, db, ct);
            return error ?? Results.NoContent();
        })
        .WithName("DeleteAdminGame")
        .WithTags("Admin")
        .WithSummary("Stop tracking a game and remove its leaderboard data and download sources.")
        .RequireAdmin();
}
