using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;

namespace RetroHiscore.Api.Features.Sync;

public sealed record GameLeaderboardSyncStatusDto(
    string Tier,
    DateTimeOffset? GroupLastPlayedAt,
    DateTimeOffset? LeaderboardSyncNextDueAt,
    int LeaderboardSyncIntervalMinutes,
    bool LeaderboardSyncIsDue,
    bool LeaderboardSyncForcedCold);

public static class GameLeaderboardSyncStatusQuery
{
    public sealed record GameSyncInput(
        Guid GameId,
        int RaGameId,
        DateTimeOffset? LeaderboardScoresSyncedAt,
        bool ForceColdLeaderboardSync);

    public static async Task<Dictionary<Guid, GameLeaderboardSyncStatusDto>> GetForGamesAsync(
        AppDbContext db,
        ISyncSettingsStore syncSettingsStore,
        IReadOnlyList<GameSyncInput> games,
        CancellationToken cancellationToken = default)
    {
        if (games.Count == 0)
        {
            return new Dictionary<Guid, GameLeaderboardSyncStatusDto>();
        }

        var policy = await syncSettingsStore.GetLeaderboardPolicyAsync(cancellationToken);
        var utcNow = DateTimeOffset.UtcNow;
        var raGameIds = games.Select(g => g.RaGameId).Distinct().ToList();
        var activityByRaGameId = await LoadMaxLastPlayedByRaGameIdAsync(db, raGameIds, cancellationToken);

        var result = new Dictionary<Guid, GameLeaderboardSyncStatusDto>(games.Count);
        foreach (var game in games)
        {
            activityByRaGameId.TryGetValue(game.RaGameId, out var maxPlayed);
            var schedule = LeaderboardSyncSchedule.Evaluate(
                utcNow,
                game.LeaderboardScoresSyncedAt,
                maxPlayed,
                policy,
                game.ForceColdLeaderboardSync);

            var intervalMinutes = schedule.Tier == LeaderboardSyncTier.Hot
                ? policy.HotIntervalMinutes
                : policy.ColdIntervalMinutes;

            result[game.GameId] = new GameLeaderboardSyncStatusDto(
                schedule.Tier.ToString(),
                maxPlayed,
                schedule.NextDueAt,
                intervalMinutes,
                schedule.IsDue,
                game.ForceColdLeaderboardSync);
        }

        return result;
    }

    public static async Task<Dictionary<int, DateTimeOffset?>> LoadMaxLastPlayedByRaGameIdAsync(
        AppDbContext db,
        IReadOnlyList<int> raGameIds,
        CancellationToken cancellationToken = default)
    {
        if (raGameIds.Count == 0)
        {
            return new Dictionary<int, DateTimeOffset?>();
        }

        var raSet = raGameIds.Distinct().ToList();
        return await db.MemberRecentGamePlays
            .AsNoTracking()
            .Where(p => raSet.Contains(p.RaGameId))
            .GroupBy(p => p.RaGameId)
            .Select(g => new { RaGameId = g.Key, MaxLastPlayedAt = g.Max(p => p.LastPlayedAt) })
            .ToDictionaryAsync(x => x.RaGameId, x => (DateTimeOffset?)x.MaxLastPlayedAt, cancellationToken);
    }
}
