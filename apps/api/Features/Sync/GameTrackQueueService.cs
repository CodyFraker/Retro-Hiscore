using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.Sync;

public interface IGameTrackQueueService
{
    Task EnqueueEligibleAsync(DateTimeOffset syncedAt, CancellationToken cancellationToken = default);
}

public sealed class GameTrackQueueService(
    AppDbContext db,
    IRaApiClient raApiClient,
    IRaApiKeyPool apiKeyPool,
    IOptions<SyncOptions> syncOptions,
    ILogger<GameTrackQueueService> logger) : IGameTrackQueueService
{
    private readonly SyncOptions _syncOptions = syncOptions.Value;

    public async Task EnqueueEligibleAsync(DateTimeOffset syncedAt, CancellationToken cancellationToken = default)
    {
        var plays = await db.MemberRecentGamePlays
            .Include(p => p.Member)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (plays.Count == 0)
        {
            return;
        }

        var trackedIds = await db.Games
            .AsNoTracking()
            .Select(g => g.RaGameId)
            .ToListAsync(cancellationToken);
        var trackedSet = trackedIds.ToHashSet();

        var blockedQueue = await db.GameTrackQueues
            .AsNoTracking()
            .Where(q => q.Status == GameTrackQueueStatus.Pending || q.Status == GameTrackQueueStatus.Rejected)
            .Select(q => q.RaGameId)
            .ToListAsync(cancellationToken);
        var blockedSet = blockedQueue.ToHashSet();

        var byGame = plays.GroupBy(p => p.RaGameId).ToList();
        var probeBudget = _syncOptions.MaxLeaderboardEligibilityProbesPerSync;
        var probesUsed = 0;

        foreach (var group in byGame)
        {
            var raGameId = group.Key;
            if (trackedSet.Contains(raGameId) || blockedSet.Contains(raGameId))
            {
                continue;
            }

            var snapshot = group.OrderByDescending(p => p.LastPlayedAt).First();
            var eligible = group.Any(p => p.NumAchieved > 0);

            if (!eligible && probesUsed < probeBudget)
            {
                foreach (var play in group.OrderByDescending(p => p.LastPlayedAt))
                {
                    if (probesUsed >= probeBudget)
                    {
                        break;
                    }

                    var member = play.Member;
                    if (string.IsNullOrWhiteSpace(member.RaUsername))
                    {
                        continue;
                    }

                    var identity = !string.IsNullOrWhiteSpace(member.RaUlid) ? member.RaUlid! : member.RaUsername!;
                    probesUsed++;
                    try
                    {
                        var boards = await apiKeyPool.ExecuteAsync(
                            Array.Empty<string>(),
                            (key, ct) => raApiClient.GetUserGameLeaderboardsAsync(raGameId, identity, key, ct),
                            cancellationToken);

                        if (boards.Any(b => b.UserEntry?.Score is not null))
                        {
                            eligible = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogDebug(
                            ex,
                            "Leaderboard eligibility probe failed for game {GameId} member {Member}",
                            raGameId,
                            member.RaUsername);
                    }
                }
            }

            if (!eligible)
            {
                continue;
            }

            db.GameTrackQueues.Add(new GameTrackQueue
            {
                RaGameId = raGameId,
                Status = GameTrackQueueStatus.Pending,
                Title = snapshot.Title,
                ConsoleName = snapshot.ConsoleName,
                EnqueuedAt = syncedAt,
                Source = GameTrackQueueSource.RecentPlay
            });
            blockedSet.Add(raGameId);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
