using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.Admin;

public sealed class AdminOpsBuilder(
    AppDbContext db,
    ILeaderboardSyncService leaderboardSyncService,
    ISyncSettingsStore syncSettingsStore,
    IOptions<RaOptions> raOptions,
    IOptions<HangfireDashboardOptions> hangfireOptions,
    IAdminSchedulerReader schedulerReader)
{
    private const int RecentRunLimit = 15;

    public async Task<AdminOpsDto> BuildAsync(CancellationToken cancellationToken = default)
    {
        var policy = await syncSettingsStore.GetLeaderboardPolicyAsync(cancellationToken);
        var intervalMinutes = Math.Clamp(policy.HotIntervalMinutes, 1, 60);
        var recurringJobSettings = await syncSettingsStore.GetRecurringJobsAsync(cancellationToken);
        var configuredByJobId = recurringJobSettings.ToDictionary(j => j.JobId);

        var recentRuns = await db.SyncRuns
            .Include(r => r.Game)
            .Include(r => r.Member)
            .OrderByDescending(r => r.StartedAt)
            .Take(RecentRunLimit)
            .Select(r => new AdminSyncRunDto(
                r.Id,
                r.Kind.ToString(),
                r.Trigger.ToString(),
                r.Status.ToString(),
                r.StartedAt,
                r.FinishedAt,
                r.Error,
                r.Game != null ? r.Game.RaGameId : null,
                r.Game != null ? r.Game.Title : null,
                r.Member != null
                    ? (r.Member.DisplayName ?? r.Member.RaUsername ?? r.Member.DiscordId)
                    : null))
            .ToListAsync(cancellationToken);

        var latestLeaderboard = await db.SyncRuns
            .Where(r => r.Kind == SyncKind.LeaderboardScores)
            .OrderByDescending(r => r.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var lastSuccessfulLeaderboard = await db.SyncRuns
            .Where(r => r.Kind == SyncKind.LeaderboardScores && r.Status == SyncRunStatus.Succeeded)
            .OrderByDescending(r => r.FinishedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var leaderboardInProgress = await db.SyncRuns
            .AnyAsync(
                r => r.Kind == SyncKind.LeaderboardScores
                    && r.Status == SyncRunStatus.Running
                    && r.FinishedAt == null,
                cancellationToken);

        var overallStatus = DeriveOverallStatus(latestLeaderboard);

        leaderboardSyncService.IsManualCooldownActive(out var manualCooldownUntil);

        var recurringJobs = EnrichRecurringJobs(schedulerReader.GetRecurringJobs(), configuredByJobId);
        var dispatchRecurring = recurringJobs.FirstOrDefault(j => j.JobId == SyncRecurringJobIds.LeaderboardDispatch);
        configuredByJobId.TryGetValue(SyncRecurringJobIds.LeaderboardDispatch, out var dispatchSettings);

        DateTimeOffset? nextScheduledAt = dispatchRecurring?.NextExecution is { } next
            ? new DateTimeOffset(DateTime.SpecifyKind(next, DateTimeKind.Utc))
            : null;

        if (nextScheduledAt is null && lastSuccessfulLeaderboard?.FinishedAt is { } lastFinished)
        {
            var dispatchMinutes = Math.Clamp(dispatchSettings?.IntervalMinutes ?? 5, 1, 60);
            nextScheduledAt = lastFinished.AddMinutes(dispatchMinutes);
        }

        var memberRows = await db.Members
            .Where(m => m.DiscordId != null)
            .Select(m => new
            {
                Label = m.RaUsername ?? m.DisplayName ?? m.DiscordId ?? "Pending",
                DisplayName = m.DisplayName ?? m.RaUsername ?? m.DiscordId ?? "Pending",
                HasApiKey = m.RaApiKey != null && m.RaApiKey != "",
                BoardsWithScore = m.Entries.Count,
                LastEntrySyncedAt = m.Entries.Max(e => (DateTimeOffset?)e.SyncedAt)
            })
            .OrderBy(m => m.HasApiKey ? 1 : 0)
            .ThenBy(m => m.Label)
            .ToListAsync(cancellationToken);

        var membersWithKey = memberRows.Count(m => m.HasApiKey);
        var memberDtos = memberRows
            .Select(m => new AdminMemberCoverageDto(
                m.Label,
                m.DisplayName,
                m.HasApiKey,
                m.BoardsWithScore,
                m.LastEntrySyncedAt))
            .ToList();

        var distinctMemberKeys = await db.Members
            .Where(m => m.RaApiKey != null && m.RaApiKey != "")
            .Select(m => m.RaApiKey!)
            .Distinct()
            .CountAsync(cancellationToken);

        var sharedKeyConfigured = !string.IsNullOrWhiteSpace(raOptions.Value.ApiKey);
        var keysInPool = RaApiKeyPool.BuildKeyOrder(
            [],
            raOptions.Value.ApiKey,
            await db.Members
                .Where(m => m.RaApiKey != null && m.RaApiKey != "")
                .Select(m => m.RaApiKey!)
                .Distinct()
                .ToListAsync(cancellationToken)).Count;

        var hangfirePath = hangfireOptions.Value.DashboardPath.TrimEnd('/');
        if (!hangfirePath.StartsWith('/'))
        {
            hangfirePath = "/" + hangfirePath;
        }

        var hangfireDashboardUrl = string.IsNullOrWhiteSpace(hangfireOptions.Value.DashboardSecret)
            ? hangfirePath
            : $"{hangfirePath}?secret={Uri.EscapeDataString(hangfireOptions.Value.DashboardSecret)}";

        var lastSyncByKind = await BuildLastSyncByKindAsync(cancellationToken);

        return new AdminOpsDto(
            new AdminOpsHealthDto(
                overallStatus,
                leaderboardInProgress,
                lastSuccessfulLeaderboard?.FinishedAt,
                intervalMinutes,
                nextScheduledAt,
                manualCooldownUntil),
            lastSyncByKind,
            recentRuns,
            new AdminMemberCoverageSummaryDto(membersWithKey, memberRows.Count),
            memberDtos,
            new AdminSchedulerDto(recurringJobs),
            new AdminOpsConfigDto(
                sharedKeyConfigured,
                distinctMemberKeys,
                keysInPool,
                hangfireDashboardUrl));
    }

    private static IReadOnlyList<RecurringJobSnapshotDto> EnrichRecurringJobs(
        IReadOnlyList<RecurringJobSnapshotDto> snapshots,
        IReadOnlyDictionary<string, Domain.SyncRecurringJob> configuredByJobId)
    {
        return snapshots
            .Select(job =>
            {
                if (!configuredByJobId.TryGetValue(job.JobId, out var configured))
                {
                    return job;
                }

                return job with
                {
                    ConfiguredIntervalMinutes = configured.IntervalMinutes,
                    ConfiguredIntervalDays = configured.IntervalDays
                };
            })
            .ToList();
    }

    private async Task<IReadOnlyList<AdminLastSyncByKindDto>> BuildLastSyncByKindAsync(
        CancellationToken cancellationToken)
    {
        var kinds = Enum.GetValues<SyncKind>();
        var results = new List<AdminLastSyncByKindDto>(kinds.Length);

        foreach (var kind in kinds)
        {
            var last = await db.SyncRuns
                .Where(r => r.Kind == kind)
                .OrderByDescending(r => r.StartedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (last is null)
            {
                results.Add(new AdminLastSyncByKindDto(kind.ToString(), null, null, null));
                continue;
            }

            results.Add(new AdminLastSyncByKindDto(
                kind.ToString(),
                last.Status.ToString(),
                last.StartedAt,
                last.FinishedAt));
        }

        return results;
    }

    private static string DeriveOverallStatus(SyncRun? latestLeaderboard)
    {
        if (latestLeaderboard is null)
        {
            return "Failing";
        }

        return latestLeaderboard.Status switch
        {
            SyncRunStatus.Succeeded => "OK",
            SyncRunStatus.PartialSuccess => "Degraded",
            SyncRunStatus.Failed => "Failing",
            SyncRunStatus.Running => "OK",
            _ => "Failing"
        };
    }
}

public sealed record AdminLastSyncByKindDto(
    string Kind,
    string? Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt);

public sealed record AdminOpsDto(
    AdminOpsHealthDto Health,
    IReadOnlyList<AdminLastSyncByKindDto> LastSyncByKind,
    IReadOnlyList<AdminSyncRunDto> RecentRuns,
    AdminMemberCoverageSummaryDto MemberCoverageSummary,
    IReadOnlyList<AdminMemberCoverageDto> Members,
    AdminSchedulerDto Scheduler,
    AdminOpsConfigDto Config);

public sealed record AdminOpsHealthDto(
    string OverallStatus,
    bool LeaderboardSyncInProgress,
    DateTimeOffset? LastSuccessfulLeaderboardSyncAt,
    int IntervalMinutes,
    DateTimeOffset? EstimatedNextScheduledSyncAt,
    DateTimeOffset? ManualCooldownUntil);

public sealed record AdminSyncRunDto(
    Guid Id,
    string Kind,
    string Trigger,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    string? Error,
    int? RaGameId,
    string? GameTitle,
    string? MemberDisplayName);

public sealed record AdminMemberCoverageSummaryDto(int MembersWithApiKey, int TotalMembers);

public sealed record AdminMemberCoverageDto(
    string RaUsername,
    string DisplayName,
    bool HasApiKey,
    int BoardsWithScore,
    DateTimeOffset? LastEntrySyncedAt);

public sealed record AdminSchedulerDto(IReadOnlyList<RecurringJobSnapshotDto> RecurringJobs);

public sealed record AdminOpsConfigDto(
    bool SharedCatalogKeyConfigured,
    int DistinctMemberApiKeys,
    int KeysInPool,
    string HangfireDashboardUrl);
