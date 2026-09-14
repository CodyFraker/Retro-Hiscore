using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Games;

public static class AddGameEndpoint
{
    public static RouteHandlerBuilder MapAddGame(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/games", async (
            AddGameRequest request,
            HttpRequest httpRequest,
            AppDbContext db,
            IRaApiClient raApiClient,
            IRaApiKeyPool apiKeyPool,
            ILeaderboardSyncService leaderboardSync,
            IConsoleIconSyncService consoleIconSync,
            IOptions<RaOptions> raOptions,
            CancellationToken ct) =>
        {
            if (request.RaGameId <= 0)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.RaGameId)] = ["raGameId must be a positive integer."]
                });
            }

            var exists = await db.Games.AnyAsync(g => g.RaGameId == request.RaGameId, ct);
            if (exists)
            {
                return Results.Conflict(new { message = $"Game {request.RaGameId} is already tracked." });
            }

            var payload = await apiKeyPool.ExecuteAsync(
                [raOptions.Value.ApiKey],
                (key, token) => raApiClient.GetGameAsync(request.RaGameId, key, token),
                ct);
            if (string.IsNullOrWhiteSpace(payload.Title))
            {
                return Results.NotFound(new { message = $"Game {request.RaGameId} was not found on RetroAchievements." });
            }

            var syncedAt = DateTimeOffset.UtcNow;
            var game = new Game
            {
                RaGameId = request.RaGameId,
                Title = payload.Title.Trim()
            };
            GameMetadataSyncService.ApplyMetadata(game, payload, syncedAt);

            db.Games.Add(game);
            await db.SaveChangesAsync(ct);

            await leaderboardSync.SyncGameAsync(game, ct);

            if (game.ConsoleId is int consoleId)
            {
                await consoleIconSync.EnsureConsoleIconAsync(consoleId, ct);
            }

            var leaderboardCount = await db.Leaderboards.CountAsync(l => l.GameId == game.Id, ct);
            var images = RaMediaUrl.FromGame(game, raOptions.Value.MediaBaseUrl);
            var console = game.ConsoleId is null
                ? null
                : await db.Consoles.AsNoTracking().FirstOrDefaultAsync(c => c.RaConsoleId == game.ConsoleId, ct);
            var requestBase = $"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}";

            return Results.Created($"/api/games/{game.RaGameId}/leaderboards", new GameDto(
                game.Id,
                game.RaGameId,
                game.Title,
                game.ConsoleName,
                ConsoleIconSyncService.ToAbsoluteUrl(game.ConsoleId, console?.IconFileName, requestBase),
                images.ImageBoxArtUrl,
                images.ImageIconUrl,
                images.ImageTitleUrl,
                images.ImageIngameUrl,
                leaderboardCount));
        })
        .WithName("AddGame")
        .WithTags("Games")
        .WithSummary("Track a new RetroAchievements game by id and sync its metadata and member scores.")
        .RequireApiAuth();
}

public sealed record AddGameRequest(int RaGameId);
