using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;

namespace RetroHiscore.Api.Data;

public static class SeedData
{
    private static readonly int[] DefaultGameIds = [38130, 2291, 789];

    public static async Task EnsureSeededAsync(
        AppDbContext db,
        RaOptions raOptions,
        CancellationToken cancellationToken = default)
    {
        var gameIds = (raOptions.TrackedGameIds is { Count: > 0 }
                ? raOptions.TrackedGameIds.AsEnumerable()
                : DefaultGameIds)
            .Distinct()
            .ToList();

        var existingGameIds = await db.Games
            .Select(g => g.RaGameId)
            .ToListAsync(cancellationToken);

        foreach (var gameId in gameIds.Except(existingGameIds))
        {
            db.Games.Add(new Game
            {
                RaGameId = gameId,
                Title = $"Game {gameId}"
            });
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
