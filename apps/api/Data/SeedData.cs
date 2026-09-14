using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Data;

public static class SeedData
{
    private static readonly int[] DefaultGameIds = [38130, 2291, 789];

    public static async Task EnsureSeededAsync(
        AppDbContext db,
        RaOptions raOptions,
        AuthOptions authOptions,
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

        await BootstrapAdminMembersAsync(db, authOptions, cancellationToken);
    }

    private static async Task BootstrapAdminMembersAsync(
        AppDbContext db,
        AuthOptions authOptions,
        CancellationToken cancellationToken)
    {
        if (authOptions.AdminDiscordUserIds is not { Count: > 0 })
        {
            return;
        }

        var existingByDiscord = await db.Members
            .Where(m => m.DiscordId != null)
            .ToDictionaryAsync(m => m.DiscordId!, StringComparer.Ordinal, cancellationToken);

        foreach (var discordId in authOptions.AdminDiscordUserIds
                     .Select(id => id.Trim())
                     .Where(id => !string.IsNullOrWhiteSpace(id))
                     .Distinct(StringComparer.Ordinal))
        {
            if (existingByDiscord.TryGetValue(discordId, out var member))
            {
                if (!member.IsAdmin)
                {
                    member.IsAdmin = true;
                }

                continue;
            }

            db.Members.Add(new Member
            {
                DiscordId = discordId,
                IsAdmin = true,
                DisplayName = "Admin"
            });
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
