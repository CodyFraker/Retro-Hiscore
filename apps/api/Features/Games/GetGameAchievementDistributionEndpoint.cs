using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Games;

public static class GetGameAchievementDistributionEndpoint
{
    public static RouteHandlerBuilder MapGetGameAchievementDistribution(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/games/{raGameId:int}/achievement-distribution", HandleAsync)
        .WithName("GetGameAchievementDistribution")
        .WithTags("Games")
        .WithSummary("Returns cached global achievement mastery distribution for a tracked game.")
        .RequireApiAuth();

    private static async Task<IResult> HandleAsync(
        int raGameId,
        AppDbContext db,
        CancellationToken ct)
    {
        var game = await db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.RaGameId == raGameId, ct);
        if (game is null)
        {
            return Results.NotFound();
        }

        var softcore = GameAchievementDistributionParser.ParseBuckets(game.AchievementDistributionSoftcoreJson);
        var hardcore = GameAchievementDistributionParser.ParseBuckets(game.AchievementDistributionHardcoreJson);
        var available = softcore.Count > 0 || hardcore.Count > 0;

        return Results.Ok(new GameAchievementDistributionResponse(
            raGameId,
            available,
            game.AchievementDistributionSyncedAt,
            softcore,
            hardcore));
    }
}
