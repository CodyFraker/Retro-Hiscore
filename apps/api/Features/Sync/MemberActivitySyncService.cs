using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Sync;

public interface IMemberActivitySyncService
{
    Task<SyncRun> SyncAsync(SyncTrigger trigger, CancellationToken cancellationToken = default);
}

public sealed class MemberActivitySyncService(
    AppDbContext db,
    IMemberRecentGamesSyncService memberRecentGamesSync,
    ILogger<MemberActivitySyncService> logger) : IMemberActivitySyncService
{
    public async Task<SyncRun> SyncAsync(SyncTrigger trigger, CancellationToken cancellationToken = default)
    {
        var run = new SyncRun
        {
            Kind = SyncKind.MemberActivity,
            Trigger = trigger,
            Status = SyncRunStatus.Running,
            StartedAt = DateTimeOffset.UtcNow
        };
        db.SyncRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        var syncedAt = DateTimeOffset.UtcNow;

        try
        {
            await memberRecentGamesSync.SyncAllMembersAsync(syncedAt, cancellationToken);
            run.Status = SyncRunStatus.Succeeded;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Member activity sync failed");
            run.Status = SyncRunStatus.Failed;
            run.Error = ex.Message;
        }
        finally
        {
            run.FinishedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        return run;
    }
}
