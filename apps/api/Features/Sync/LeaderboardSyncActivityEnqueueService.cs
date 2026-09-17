using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.Sync;

public interface ILeaderboardSyncActivityEnqueueService
{
    Task EnqueueForMemberChangesAsync(
        IReadOnlyList<MemberRecentGamePlayChange> changes,
        SyncTrigger trigger,
        CancellationToken cancellationToken = default);
}

public sealed class LeaderboardSyncActivityEnqueueService(
    AppDbContext db,
    ISyncSettingsStore syncSettingsStore,
    ILeaderboardSyncJobEnqueuer jobEnqueuer,
    IOptions<SyncOptions> syncOptions,
    ILogger<LeaderboardSyncActivityEnqueueService> logger) : ILeaderboardSyncActivityEnqueueService
{
    private readonly SyncOptions _syncOptions = syncOptions.Value;

    public async Task EnqueueForMemberChangesAsync(
        IReadOnlyList<MemberRecentGamePlayChange> changes,
        SyncTrigger trigger,
        CancellationToken cancellationToken = default)
    {
        if (changes.Count == 0)
        {
            return;
        }

        var trackedRaGameIds = await db.Games
            .AsNoTracking()
            .Select(g => g.RaGameId)
            .ToListAsync(cancellationToken);
        var trackedSet = trackedRaGameIds.ToHashSet();

        var policy = await syncSettingsStore.GetLeaderboardPolicyAsync(cancellationToken);
        var utcNow = DateTimeOffset.UtcNow;
        var windowStart = utcNow - TimeSpan.FromHours(policy.HotActivityWindowHours);

        foreach (var change in changes)
        {
            if (!trackedSet.Contains(change.RaGameId))
            {
                continue;
            }

            if (change.LastPlayedAt < windowStart)
            {
                continue;
            }

            if (!change.IsNewOrUpdated && !await NeedsSyncDespiteUnchangedPlayAsync(
                    change.MemberId,
                    change.RaGameId,
                    change.LastPlayedAt,
                    cancellationToken))
            {
                continue;
            }

            if (trigger == SyncTrigger.Manual
                && IsPerGameRefreshCooldownActive(change.MemberId, change.RaGameId, out _))
            {
                continue;
            }

            if (trigger != SyncTrigger.Manual
                && IsScheduledPerGameCooldownActive(change.MemberId, change.RaGameId, out _))
            {
                continue;
            }

            var game = await db.Games
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.RaGameId == change.RaGameId, cancellationToken);
            if (game is null)
            {
                continue;
            }

            jobEnqueuer.EnqueueMemberGameSync(change.RaGameId, change.MemberId, trigger);
            logger.LogInformation(
                "Enqueued member-game leaderboard sync for member {MemberId} game {RaGameId}",
                change.MemberId,
                change.RaGameId);
        }
    }

    private async Task<bool> NeedsSyncDespiteUnchangedPlayAsync(
        Guid memberId,
        int raGameId,
        DateTimeOffset lastPlayedAt,
        CancellationToken cancellationToken)
    {
        var game = await db.Games
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.RaGameId == raGameId, cancellationToken);
        if (game is null)
        {
            return false;
        }

        var lastSuccess = await db.SyncRuns
            .AsNoTracking()
            .Where(r => r.Kind == SyncKind.LeaderboardScores
                && r.GameId == game.Id
                && r.MemberId == memberId
                && r.Status == SyncRunStatus.Succeeded
                && r.FinishedAt != null)
            .OrderByDescending(r => r.FinishedAt)
            .Select(r => r.FinishedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return lastSuccess is null || lastSuccess < lastPlayedAt;
    }

    private bool IsPerGameRefreshCooldownActive(Guid memberId, int raGameId, out DateTimeOffset? availableAt)
    {
        availableAt = null;
        var game = db.Games.AsNoTracking().FirstOrDefault(g => g.RaGameId == raGameId);
        if (game is null)
        {
            return false;
        }

        var lastManual = db.SyncRuns
            .Where(r => r.Kind == SyncKind.LeaderboardScores
                && r.Trigger == SyncTrigger.Manual
                && r.GameId == game.Id
                && r.MemberId == memberId)
            .OrderByDescending(r => r.StartedAt)
            .FirstOrDefault();

        if (lastManual is null)
        {
            return false;
        }

        var cooldownEnds = lastManual.StartedAt.AddSeconds(_syncOptions.PerGameRefreshCooldownSeconds);
        if (cooldownEnds <= DateTimeOffset.UtcNow)
        {
            return false;
        }

        availableAt = cooldownEnds;
        return true;
    }

    private bool IsScheduledPerGameCooldownActive(Guid memberId, int raGameId, out DateTimeOffset? availableAt)
    {
        availableAt = null;
        var game = db.Games.AsNoTracking().FirstOrDefault(g => g.RaGameId == raGameId);
        if (game is null)
        {
            return false;
        }

        var lastRun = db.SyncRuns
            .Where(r => r.Kind == SyncKind.LeaderboardScores
                && r.GameId == game.Id
                && r.MemberId == memberId)
            .OrderByDescending(r => r.StartedAt)
            .FirstOrDefault();

        if (lastRun is null)
        {
            return false;
        }

        var cooldownEnds = lastRun.StartedAt.AddSeconds(_syncOptions.PerGameRefreshCooldownSeconds);
        if (cooldownEnds <= DateTimeOffset.UtcNow)
        {
            return false;
        }

        availableAt = cooldownEnds;
        return true;
    }
}
