using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Sync;

namespace RetroHiscore.Api.Tests;

public sealed class InlineLeaderboardSyncJobEnqueuer(IServiceScopeFactory scopeFactory) : ILeaderboardSyncJobEnqueuer
{
    public void EnqueueFullGameSync(int raGameId, SyncTrigger trigger, TimeSpan? delay = null)
    {
        using var scope = scopeFactory.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<GameLeaderboardSyncJob>();
        job.RunFullGameAsync(raGameId, trigger, CancellationToken.None).GetAwaiter().GetResult();
    }

    public void EnqueueMemberGameSync(
        int raGameId,
        Guid memberId,
        SyncTrigger trigger,
        Guid? syncRunId = null,
        TimeSpan? delay = null)
    {
        using var scope = scopeFactory.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<MemberGameLeaderboardSyncJob>();
        job.RunForMemberAsync(raGameId, memberId, trigger, syncRunId, CancellationToken.None).GetAwaiter().GetResult();
    }
}

public sealed class RecordingLeaderboardSyncJobEnqueuer : ILeaderboardSyncJobEnqueuer
{
    public List<(int RaGameId, SyncTrigger Trigger)> FullGameEnqueues { get; } = [];
    public List<(int RaGameId, Guid MemberId, SyncTrigger Trigger)> MemberEnqueues { get; } = [];

    public void EnqueueFullGameSync(int raGameId, SyncTrigger trigger, TimeSpan? delay = null)
        => FullGameEnqueues.Add((raGameId, trigger));

    public void EnqueueMemberGameSync(
        int raGameId,
        Guid memberId,
        SyncTrigger trigger,
        Guid? syncRunId = null,
        TimeSpan? delay = null)
        => MemberEnqueues.Add((raGameId, memberId, trigger));
}
