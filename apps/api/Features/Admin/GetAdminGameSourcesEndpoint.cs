using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin;

public static class GetAdminGameSourcesEndpoint
{
    public static RouteHandlerBuilder MapGetAdminGameSources(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/admin/games/{raGameId:int}/sources", async (
            int raGameId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var game = await GameSourceQueries.FindGameByRaIdAsync(db, raGameId, ct);
            if (game is null)
            {
                return Results.NotFound();
            }

            var items = await GameSourceQueries.ListForRaGameAsync(db, raGameId, ct);
            return Results.Ok(items);
        })
        .WithName("GetAdminGameSources")
        .WithTags("Admin")
        .WithSummary("Lists download mirror links for a tracked game (admin edit view).")
        .RequireAdmin();
}
