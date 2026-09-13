using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Games;

public static class DeleteGameEndpoint
{
    public static RouteHandlerBuilder MapDeleteGame(this IEndpointRouteBuilder routes)
        => routes.MapDelete("/api/games/{raGameId:int}", async (
            int raGameId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var game = await db.Games
                .Include(g => g.Leaderboards)
                .FirstOrDefaultAsync(g => g.RaGameId == raGameId, ct);

            if (game is null)
            {
                return Results.NotFound(new { message = $"Game {raGameId} is not tracked." });
            }

            db.Games.Remove(game);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("DeleteGame")
        .WithTags("Games")
        .WithSummary("Stop tracking a game and remove its leaderboard data.")
        .RequireApiAuth();
}
