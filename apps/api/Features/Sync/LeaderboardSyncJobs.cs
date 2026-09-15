using System.Collections.Concurrent;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Sync;

public sealed class MemberActivitySyncJob(IMemberActivitySyncService activitySync)
{
    [DisableConcurrentExecution(timeoutInSeconds: 60 * 30)]
    [AutomaticRetry(Attempts = 0)]
    public Task RunScheduledAsync(CancellationToken cancellationToken = default)
        => activitySync.SyncAsync(SyncTrigger.Scheduled, cancellationToken);
}

public sealed class MemberRankSyncJob(IMemberRankSyncService rankSync)
{
    [DisableConcurrentExecution(timeoutInSeconds: 60 * 30)]
    [AutomaticRetry(Attempts = 0)]
    public Task RunScheduledAsync(CancellationToken cancellationToken = default)
        => rankSync.SyncAsync(SyncTrigger.Scheduled, cancellationToken);
}

public sealed class MemberAchievementSyncJob(IMemberAchievementSyncService achievementSync)
{
    [DisableConcurrentExecution(timeoutInSeconds: 60 * 30)]
    [AutomaticRetry(Attempts = 0)]
    public Task RunScheduledAsync(CancellationToken cancellationToken = default)
        => achievementSync.SyncAsync(SyncTrigger.Scheduled, cancellationToken);
}

public sealed class LeaderboardSyncDispatchJob(ILeaderboardSyncDispatcher dispatcher)
{
    [DisableConcurrentExecution(timeoutInSeconds: 60 * 30)]
    [AutomaticRetry(Attempts = 0)]
    public Task RunScheduledAsync(CancellationToken cancellationToken = default)
        => dispatcher.DispatchDueGamesAsync(SyncTrigger.Scheduled, forceAll: false, cancellationToken);
}

public sealed class GameLeaderboardSyncJob(
    AppDbContext db,
    ILeaderboardSyncService leaderboardSync,
    ILogger<GameLeaderboardSyncJob> logger)
{
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> GameLocks = new();

    [AutomaticRetry(Attempts = 0)]
    public async Task RunFullGameAsync(int raGameId, SyncTrigger trigger, CancellationToken cancellationToken = default)
    {
        var semaphore = GameLocks.GetOrAdd(raGameId, _ => new SemaphoreSlim(1, 1));
        if (!await semaphore.WaitAsync(0, cancellationToken))
        {
            logger.LogWarning("Skipping duplicate full game sync for {RaGameId}", raGameId);
            return;
        }

        try
        {
            var game = await db.Games.FirstOrDefaultAsync(g => g.RaGameId == raGameId, cancellationToken);
            if (game is null)
            {
                logger.LogWarning("Game {RaGameId} is not tracked", raGameId);
                return;
            }

            await leaderboardSync.SyncGameWithRunAsync(game, trigger, cancellationToken);
        }
        finally
        {
            semaphore.Release();
        }
    }
}

public sealed class MemberGameLeaderboardSyncJob(
    AppDbContext db,
    ILeaderboardSyncService leaderboardSync,
    ILogger<MemberGameLeaderboardSyncJob> logger)
{
    private static readonly ConcurrentDictionary<(int RaGameId, Guid MemberId), SemaphoreSlim> MemberGameLocks = new();

    [AutomaticRetry(Attempts = 0)]
    public async Task RunForMemberAsync(
        int raGameId,
        Guid memberId,
        SyncTrigger trigger,
        Guid? syncRunId,
        CancellationToken cancellationToken = default)
    {
        var key = (raGameId, memberId);
        var semaphore = MemberGameLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        if (!await semaphore.WaitAsync(0, cancellationToken))
        {
            logger.LogWarning("Skipping duplicate member game sync for {RaGameId} member {MemberId}", raGameId, memberId);
            return;
        }

        try
        {
            var game = await db.Games.FirstOrDefaultAsync(g => g.RaGameId == raGameId, cancellationToken);
            if (game is null)
            {
                logger.LogWarning("Game {RaGameId} is not tracked", raGameId);
                return;
            }

            var member = await db.Members.FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
            if (member is null)
            {
                logger.LogWarning("Member {MemberId} not found", memberId);
                return;
            }

            await leaderboardSync.SyncMemberGameWithRunAsync(
                game,
                member,
                trigger,
                syncRunId,
                cancellationToken);
        }
        finally
        {
            semaphore.Release();
        }
    }
}
