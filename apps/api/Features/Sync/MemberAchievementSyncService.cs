using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.Sync;

public interface IMemberAchievementSyncService
{
    Task<SyncRun> SyncAsync(SyncTrigger trigger, CancellationToken cancellationToken = default);
    bool IsManualCooldownActive(out DateTimeOffset? availableAt);
}

public sealed class MemberAchievementSyncService(
    AppDbContext db,
    IMemberRaGameProgressSyncService memberRaGameProgressSync,
    IOptions<SyncOptions> syncOptions,
    ILogger<MemberAchievementSyncService> logger) : IMemberAchievementSyncService
{
    private readonly SyncOptions _syncOptions = syncOptions.Value;

    public bool IsManualCooldownActive(out DateTimeOffset? availableAt)
        => ManualSyncCooldown.IsActive(
            db,
            SyncKind.MemberAchievements,
            _syncOptions.ManualCooldownSeconds,
            out availableAt);

    public async Task<SyncRun> SyncAsync(SyncTrigger trigger, CancellationToken cancellationToken = default)
    {
        var run = new SyncRun
        {
            Kind = SyncKind.MemberAchievements,
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
                .Where(m => m.RaUsername != null && m.RaApiKey != null && m.RaApiKey != "")
                .ToListAsync(cancellationToken);

            foreach (var member in members)
            {
                try
                {
                    await memberRaGameProgressSync.SyncMemberTrackedGamesAsync(member, syncedAt, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to sync achievements for member {Member}", member.RaUsername);
                    errors.Add($"{member.RaUsername}: {ex.Message}");
                }
            }

            run.Status = errors.Count == 0
                ? SyncRunStatus.Succeeded
                : SyncRunStatus.PartialSuccess;
            run.Error = errors.Count == 0 ? null : string.Join("; ", errors);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Member achievement sync run failed");
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
