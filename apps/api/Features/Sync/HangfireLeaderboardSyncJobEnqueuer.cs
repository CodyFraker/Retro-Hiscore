using Hangfire;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Sync;

public sealed class HangfireLeaderboardSyncJobEnqueuer(IBackgroundJobClient backgroundJobs) : ILeaderboardSyncJobEnqueuer
{
    public void EnqueueFullGameSync(int raGameId, SyncTrigger trigger, TimeSpan? delay = null)
    {
        if (delay is { } d && d > TimeSpan.Zero)
        {
            backgroundJobs.Schedule<GameLeaderboardSyncJob>(
                job => job.RunFullGameAsync(raGameId, trigger, CancellationToken.None),
                d);
            return;
        }

        backgroundJobs.Enqueue<GameLeaderboardSyncJob>(
            job => job.RunFullGameAsync(raGameId, trigger, CancellationToken.None));
    }

    public void EnqueueMemberGameSync(
        int raGameId,
        Guid memberId,
        SyncTrigger trigger,
        Guid? syncRunId = null,
        TimeSpan? delay = null)
    {
        if (delay is { } d && d > TimeSpan.Zero)
        {
            backgroundJobs.Schedule<MemberGameLeaderboardSyncJob>(
                job => job.RunForMemberAsync(raGameId, memberId, trigger, syncRunId, CancellationToken.None),
                d);
            return;
        }

        backgroundJobs.Enqueue<MemberGameLeaderboardSyncJob>(
            job => job.RunForMemberAsync(raGameId, memberId, trigger, syncRunId, CancellationToken.None));
    }
}
