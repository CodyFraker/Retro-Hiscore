using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;

namespace RetroHiscore.Api.Features.Games;

public static class GameEngagedMembersQuery
{
    public static async Task<IReadOnlyList<EngagedMemberRow>> GetAsync(
        AppDbContext db,
        Guid gameId,
        int raGameId,
        CancellationToken cancellationToken = default)
    {
        var byGame = await GetForGamesAsync(db, [(gameId, raGameId)], cancellationToken);
        return byGame.TryGetValue(gameId, out var rows) ? rows : [];
    }

    public static async Task<Dictionary<Guid, IReadOnlyList<EngagedMemberRow>>> GetForGamesAsync(
        AppDbContext db,
        IReadOnlyList<(Guid GameId, int RaGameId)> games,
        CancellationToken cancellationToken = default)
    {
        if (games.Count == 0)
        {
            return new Dictionary<Guid, IReadOnlyList<EngagedMemberRow>>();
        }

        var gameIds = games.Select(g => g.GameId).Distinct().ToList();
        var raGameIdToGameId = games.ToDictionary(g => g.RaGameId, g => g.GameId);
        var raGameIds = raGameIdToGameId.Keys.ToList();

        var engagedByGame = gameIds.ToDictionary(id => id, _ => new HashSet<Guid>());

        var fromLeaderboards = await db.LeaderboardEntries
            .AsNoTracking()
            .Where(e => gameIds.Contains(e.Leaderboard.GameId))
            .Select(e => new { e.Leaderboard.GameId, e.MemberId })
            .ToListAsync(cancellationToken);

        foreach (var row in fromLeaderboards)
        {
            engagedByGame[row.GameId].Add(row.MemberId);
        }

        var fromAchievements = await db.MemberRaAchievements
            .AsNoTracking()
            .Where(m => raGameIds.Contains(m.Achievement.RaGameId))
            .Select(m => new { m.Achievement.RaGameId, m.MemberId })
            .ToListAsync(cancellationToken);

        foreach (var row in fromAchievements)
        {
            if (raGameIdToGameId.TryGetValue(row.RaGameId, out var gameId))
            {
                engagedByGame[gameId].Add(row.MemberId);
            }
        }

        var allMemberIds = engagedByGame.Values.SelectMany(s => s).Distinct().ToList();
        if (allMemberIds.Count == 0)
        {
            return gameIds.ToDictionary(id => id, _ => (IReadOnlyList<EngagedMemberRow>)[]);
        }

        var members = await db.Members
            .AsNoTracking()
            .Where(m => allMemberIds.Contains(m.Id) && m.RaUsername != null)
            .Select(m => new EngagedMemberRow(
                m.Id,
                m.RaUsername!,
                m.DisplayName ?? m.RaUsername!,
                m.AvatarUrl))
            .ToListAsync(cancellationToken);

        var memberById = members.ToDictionary(m => m.Id);

        return gameIds.ToDictionary(
            gameId => gameId,
            gameId =>
            {
                var rows = engagedByGame[gameId]
                    .Where(memberById.ContainsKey)
                    .Select(id => memberById[id])
                    .OrderBy(m => m.DisplayName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(m => m.RaUsername, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                return (IReadOnlyList<EngagedMemberRow>)rows;
            });
    }
}

public sealed record EngagedMemberRow(Guid Id, string RaUsername, string DisplayName, string? AvatarUrl);
