using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.Sync;

public interface ILeaderboardSyncDispatcher
{
    Task<int> DispatchDueGamesAsync(SyncTrigger trigger, bool forceAll, CancellationToken cancellationToken = default);
}

public sealed class LeaderboardSyncDispatcher(
    AppDbContext db,
    ILeaderboardSyncJobEnqueuer jobEnqueuer,
    ISyncSettingsStore syncSettingsStore,
    IOptions<SyncOptions> syncOptions,
    ILogger<LeaderboardSyncDispatcher> logger) : ILeaderboardSyncDispatcher
{
    public async Task<int> DispatchDueGamesAsync(
        SyncTrigger trigger,
        bool forceAll,
        CancellationToken cancellationToken = default)
    {
        var policy = await syncSettingsStore.GetLeaderboardPolicyAsync(cancellationToken);
        var staggerSeconds = syncOptions.Value.DispatchStaggerSeconds;
        var utcNow = DateTimeOffset.UtcNow;

        var games = await db.Games.AsNoTracking().ToListAsync(cancellationToken);
        if (games.Count == 0)
        {
            return 0;
        }

        var activityByRaGameId = await db.MemberRecentGamePlays
            .AsNoTracking()
            .GroupBy(p => p.RaGameId)
            .Select(g => new { RaGameId = g.Key, MaxLastPlayedAt = g.Max(p => p.LastPlayedAt) })
            .ToDictionaryAsync(x => x.RaGameId, x => (DateTimeOffset?)x.MaxLastPlayedAt, cancellationToken);

        var enqueued = 0;
        var staggerIndex = 0;

        foreach (var game in games)
        {
            activityByRaGameId.TryGetValue(game.RaGameId, out var maxPlayed);
            var schedule = LeaderboardSyncSchedule.Evaluate(
                utcNow,
                game.LeaderboardScoresSyncedAt,
                maxPlayed,
                policy);

            if (!forceAll && !schedule.IsDue)
            {
                continue;
            }

            TimeSpan? delay = staggerIndex == 0
                ? null
                : TimeSpan.FromSeconds(staggerIndex * Math.Max(0, staggerSeconds));
            staggerIndex++;

            jobEnqueuer.EnqueueFullGameSync(game.RaGameId, trigger, delay);
            enqueued++;
            logger.LogInformation(
                "Enqueued leaderboard sync for game {RaGameId} tier {Tier}",
                game.RaGameId,
                schedule.Tier);
        }

        return enqueued;
    }
}
