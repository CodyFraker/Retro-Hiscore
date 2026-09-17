using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Sync;

public sealed class NullLeaderboardSyncJobEnqueuer : ILeaderboardSyncJobEnqueuer
{
    public void EnqueueGameShellSync(int raGameId, SyncTrigger trigger, TimeSpan? delay = null)
    {
    }

    public void EnqueueMemberGameSync(
        int raGameId,
        Guid memberId,
        SyncTrigger trigger,
        Guid? syncRunId = null,
        TimeSpan? delay = null)
    {
    }
}
