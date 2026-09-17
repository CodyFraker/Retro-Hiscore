using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Sync;

public interface ILeaderboardSyncJobEnqueuer
{
    void EnqueueGameShellSync(int raGameId, SyncTrigger trigger, TimeSpan? delay = null);

    void EnqueueMemberGameSync(
        int raGameId,
        Guid memberId,
        SyncTrigger trigger,
        Guid? syncRunId = null,
        TimeSpan? delay = null);
}
