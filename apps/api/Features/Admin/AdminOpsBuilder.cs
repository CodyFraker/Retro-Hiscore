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
    IOptions<SyncOptions> syncOptions,
    IOptions<RaOptions> raOptions,
    IOptions<HangfireDashboardOptions> hangfireOptions,
    IAdminSchedulerReader schedulerReader)
{
    private const int RecentRunLimit = 15;

    public async Task<AdminOpsDto> BuildAsync(CancellationToken cancellationToken = default)
    {
        var sync = syncOptions.Value;
        var intervalMinutes = Math.Clamp(sync.IntervalMinutes, 1, 60);

        var recentRuns = await db.SyncRuns
            .OrderByDescending(r => r.StartedAt)
            .Take(RecentRunLimit)
            .Select(r => new AdminSyncRunDto(
                r.Id,
                r.Kind.ToString(),
                r.Trigger.ToString(),
                r.Status.ToString(),
                r.StartedAt,
                r.FinishedAt,
                r.Error))
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

        var recurringJobs = schedulerReader.GetRecurringJobs();
        var leaderboardRecurring = recurringJobs.FirstOrDefault(j => j.JobId == "ra-leaderboard-sync");

        DateTimeOffset? nextScheduledAt = leaderboardRecurring?.NextExecution is { } next
            ? new DateTimeOffset(DateTime.SpecifyKind(next, DateTimeKind.Utc))
            : null;

        if (nextScheduledAt is null && lastSuccessfulLeaderboard?.FinishedAt is { } lastFinished)
        {
            nextScheduledAt = lastFinished.AddMinutes(intervalMinutes);
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

        return new AdminOpsDto(
            new AdminOpsHealthDto(
                overallStatus,
                leaderboardInProgress,
                lastSuccessfulLeaderboard?.FinishedAt,
                intervalMinutes,
                nextScheduledAt,
                manualCooldownUntil),
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

public sealed record AdminOpsDto(
    AdminOpsHealthDto Health,
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
    string? Error);

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
