using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Games;

public static class GetGameSourcesEndpoint
{
    public static RouteHandlerBuilder MapGetGameSources(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/games/{raGameId:int}/sources", async (
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
        .WithName("GetGameSources")
        .WithTags("Games")
        .WithSummary("Lists download mirror links for a tracked game.")
        .RequireApiAuth();
}
