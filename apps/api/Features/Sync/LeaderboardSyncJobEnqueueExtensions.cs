using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Sync;

public static class LeaderboardSyncJobEnqueueExtensions
{
    public static async Task EnqueueGameShellAndEligibleMembersAsync(
        ILeaderboardSyncJobEnqueuer enqueuer,
        AppDbContext db,
        ISyncSettingsStore syncSettingsStore,
        int raGameId,
        SyncTrigger trigger,
        TimeSpan? shellDelay = null,
        CancellationToken cancellationToken = default)
    {
        enqueuer.EnqueueGameShellSync(raGameId, trigger, shellDelay);
        await EnqueueEligibleMemberGameSyncsAsync(
            enqueuer,
            db,
            syncSettingsStore,
            raGameId,
            trigger,
            cancellationToken);
    }

    public static async Task EnqueueEligibleMemberGameSyncsAsync(
        ILeaderboardSyncJobEnqueuer enqueuer,
        AppDbContext db,
        ISyncSettingsStore syncSettingsStore,
        int raGameId,
        SyncTrigger trigger,
        CancellationToken cancellationToken = default)
    {
        var policy = await syncSettingsStore.GetLeaderboardPolicyAsync(cancellationToken);
        var memberIds = await LeaderboardSyncMemberEligibility.GetEligibleMemberIdsAsync(
            db,
            raGameId,
            DateTimeOffset.UtcNow,
            policy,
            cancellationToken);

        foreach (var memberId in memberIds)
        {
            enqueuer.EnqueueMemberGameSync(raGameId, memberId, trigger);
        }
    }
}
