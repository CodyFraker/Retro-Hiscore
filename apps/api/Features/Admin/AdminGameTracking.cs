using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;

namespace RetroHiscore.Api.Features.Admin;

public static class AdminGameTracking
{
    public static async Task<(Game? Game, IResult? Error)> AddGameAsync(
        int raGameId,
        AppDbContext db,
        IRaApiClient raApiClient,
        IRaApiKeyPool apiKeyPool,
        ILeaderboardSyncService leaderboardSync,
        IConsoleIconSyncService consoleIconSync,
        IOptions<RaOptions> raOptions,
        CancellationToken ct,
        bool syncMemberScores = true)
    {
        if (raGameId <= 0)
        {
            return (null, Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(raGameId)] = ["raGameId must be a positive integer."]
            }));
        }

        var exists = await db.Games.AnyAsync(g => g.RaGameId == raGameId, ct);
        if (exists)
        {
            return (null, Results.Conflict(new { message = $"Game {raGameId} is already tracked." }));
        }

        var payload = await apiKeyPool.ExecuteAsync(
            [raOptions.Value.ApiKey],
            (key, token) => raApiClient.GetGameAsync(raGameId, key, token),
            ct);
        if (string.IsNullOrWhiteSpace(payload.Title))
        {
            return (null, Results.NotFound(new { message = $"Game {raGameId} was not found on RetroAchievements." }));
        }

        var syncedAt = DateTimeOffset.UtcNow;
        var game = new Game
        {
            RaGameId = raGameId,
            Title = payload.Title.Trim()
        };
        GameMetadataSyncService.ApplyMetadata(game, payload, syncedAt);

        db.Games.Add(game);
        await db.SaveChangesAsync(ct);

        if (syncMemberScores)
        {
            await leaderboardSync.SyncGameAsync(game, ct);
        }

        if (game.ConsoleId is int consoleId)
        {
            await consoleIconSync.EnsureConsoleIconAsync(consoleId, ct);
        }

        return (game, null);
    }

    public static async Task<IResult?> DeleteGameAsync(
        int raGameId,
        AppDbContext db,
        CancellationToken ct)
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
        return null;
    }

    public static async Task<(Game? Game, IResult? Error)> RefreshGameAsync(
        int raGameId,
        AppDbContext db,
        IGameMetadataSyncService gameMetadataSync,
        CancellationToken ct)
    {
        var game = await db.Games.FirstOrDefaultAsync(g => g.RaGameId == raGameId, ct);
        if (game is null)
        {
            return (null, Results.NotFound(new { message = $"Game {raGameId} is not tracked." }));
        }

        await gameMetadataSync.SyncGameAsync(game, ct);
        return (game, null);
    }
}
