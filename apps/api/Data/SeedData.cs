using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;

namespace RetroHiscore.Api.Data;

public static class SeedData
{
    private static readonly int[] DefaultGameIds = [38130, 2291, 789];
    private static readonly string[] DefaultUsernames = ["ShrimpPoboy", "beefboybilly", "xXScubXx"];

    public static async Task EnsureSeededAsync(AppDbContext db, RaOptions options, CancellationToken cancellationToken = default)
    {
        var gameIds = (options.TrackedGameIds is { Count: > 0 }
                ? options.TrackedGameIds.AsEnumerable()
                : DefaultGameIds)
            .Distinct()
            .ToList();

        var usernames = (options.TrackedUsernames is { Count: > 0 }
                ? options.TrackedUsernames.AsEnumerable()
                : DefaultUsernames)
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Distinct(StringComparer.OrdinalIgnoreCase)
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

        var existingUsernames = await db.Members
            .Select(m => m.RaUsername)
            .ToListAsync(cancellationToken);

        var existingSet = new HashSet<string>(existingUsernames, StringComparer.OrdinalIgnoreCase);
        foreach (var username in usernames.Where(u => !existingSet.Contains(u)))
        {
            db.Members.Add(new Member
            {
                RaUsername = username,
                DisplayName = username
            });
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        await ApplyMemberLinksAsync(db, options, cancellationToken);
    }

    private static async Task ApplyMemberLinksAsync(
        AppDbContext db,
        RaOptions options,
        CancellationToken cancellationToken)
    {
        if (options.MemberLinks is not { Count: > 0 })
        {
            return;
        }

        var members = await db.Members.ToListAsync(cancellationToken);
        var byUsername = members.ToDictionary(m => m.RaUsername, StringComparer.OrdinalIgnoreCase);

        foreach (var link in options.MemberLinks)
        {
            if (string.IsNullOrWhiteSpace(link.DiscordId) || string.IsNullOrWhiteSpace(link.RaUsername))
            {
                continue;
            }

            if (!byUsername.TryGetValue(link.RaUsername, out var member))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(member.DiscordId))
            {
                member.DiscordId = link.DiscordId.Trim();
            }
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
