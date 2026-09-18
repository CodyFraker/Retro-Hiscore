using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Infrastructure;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.Sync;

public interface IMemberRankSyncService
{
    Task<SyncRun> SyncAsync(SyncTrigger trigger, CancellationToken cancellationToken = default);
    bool IsManualCooldownActive(out DateTimeOffset? availableAt);
}

public sealed class MemberRankSyncService(
    AppDbContext db,
    IMemberRaRankSnapshotSync memberRaRankSnapshotSync,
    IMemberRaGameProgressSyncService memberRaGameProgressSync,
    IOptions<SyncOptions> syncOptions,
    ILogger<MemberRankSyncService> logger) : IMemberRankSyncService
{
    private readonly SyncOptions _syncOptions = syncOptions.Value;

    public bool IsManualCooldownActive(out DateTimeOffset? availableAt)
        => ManualSyncCooldown.IsActive(
            db,
            SyncKind.MemberRank,
            _syncOptions.ManualCooldownSeconds,
            out availableAt);

    public async Task<SyncRun> SyncAsync(SyncTrigger trigger, CancellationToken cancellationToken = default)
    {
        var run = new SyncRun
        {
            Kind = SyncKind.MemberRank,
            Trigger = trigger,
            Status = SyncRunStatus.Running,
            StartedAt = DateTimeOffset.UtcNow
        };
        db.SyncRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        var errors = new List<string>();
        var syncedAt = DateTimeOffset.UtcNow;

        try
        {
            var members = await db.Members
                .Where(m => m.RaUsername != null)
                .ToListAsync(cancellationToken);

            foreach (var member in members)
            {
                try
                {
                    await memberRaRankSnapshotSync.SyncMemberAsync(member, syncedAt, cancellationToken);
                    await memberRaGameProgressSync.SyncMemberTrackedGamesAsync(
                        member,
                        syncedAt,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to sync RA site rank for member {Member}", member.RaUsername);
                    errors.Add($"{member.RaUsername} RA rank: {ex.Message}");
                }
            }

            run.Status = errors.Count == 0
                ? SyncRunStatus.Succeeded
                : SyncRunStatus.PartialSuccess;
            run.Error = errors.Count == 0 ? null : string.Join("; ", errors);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Member rank sync run failed");
            run.Status = SyncRunStatus.Failed;
            run.Error = ex.Message;
        }
        finally
        {
            run.FinishedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            SyncRunMetrics.Record(run);
        }

        return run;
    }
}
