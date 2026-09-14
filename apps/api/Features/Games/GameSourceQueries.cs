using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Games;

public static class GameSourceQueries
{
    public static async Task<Game?> FindGameByRaIdAsync(AppDbContext db, int raGameId, CancellationToken ct)
        => await db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.RaGameId == raGameId, ct);

    public static async Task<IReadOnlyList<GameSourceDto>> ListForRaGameAsync(
        AppDbContext db,
        int raGameId,
        CancellationToken ct)
    {
        var sources = await db.GameSources
            .AsNoTracking()
            .Where(s => s.Game.RaGameId == raGameId)
            .ToListAsync(ct);

        return GameSourceMapping.ToDtos(sources);
    }
}
