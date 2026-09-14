using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.Sync;

public interface IMemberRankSyncService
{
    Task<SyncRun> SyncAsync(SyncTrigger trigger, CancellationToken cancellationToken = default);
}

public sealed class MemberRankSyncService(
    AppDbContext db,
    IRaApiClient raApiClient,
    IRaApiKeyPool apiKeyPool,
    IMemberRaGameProgressSyncService memberRaGameProgressSync,
    ILogger<MemberRankSyncService> logger) : IMemberRankSyncService
{
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
                    var summary = await SyncMemberRaRankSnapshotAsync(member, syncedAt, cancellationToken);
                    if (summary is not null)
                    {
                        await memberRaGameProgressSync.SyncMemberGamesFromSummaryAsync(
                            member,
                            summary,
                            syncedAt,
                            cancellationToken);
                    }
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
        }

        return run;
    }

    private async Task<RaUserSummaryDto?> SyncMemberRaRankSnapshotAsync(
        Member member,
        DateTimeOffset syncedAt,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(member.RaUsername))
        {
            return null;
        }

        var preferredKeys = string.IsNullOrWhiteSpace(member.RaApiKey)
            ? Array.Empty<string>()
            : new[] { member.RaApiKey };

        var target = !string.IsNullOrWhiteSpace(member.RaUlid) ? member.RaUlid! : member.RaUsername!;
        var summary = await apiKeyPool.ExecuteAsync(
            preferredKeys,
            (key, ct) => raApiClient.GetUserSummaryAsync(
                target,
                key,
                recentGamesCount: 3,
                recentAchievementsCount: 0,
                cancellationToken: ct),
            cancellationToken);

        if (summary is null)
        {
            return null;
        }

        var hasMetric = summary.Rank is not null
            || summary.TotalPoints is not null
            || summary.TotalTruePoints is not null
            || summary.TotalSoftcorePoints is not null;

        if (hasMetric)
        {
            db.MemberRaRankSnapshots.Add(new MemberRaRankSnapshot
            {
                MemberId = member.Id,
                Rank = summary.Rank,
                TotalRanked = summary.TotalRanked,
                TotalPoints = summary.TotalPoints,
                TotalTruePoints = summary.TotalTruePoints,
                TotalSoftcorePoints = summary.TotalSoftcorePoints,
                SyncedAt = syncedAt
            });

            await db.SaveChangesAsync(cancellationToken);
        }

        return summary;
    }
}
