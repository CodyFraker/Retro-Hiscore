using Microsoft.EntityFrameworkCore;
using Npgsql;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Admin;

namespace RetroHiscore.Api.Features.Sync;

public sealed class SyncSettingsStore(AppDbContext db) : ISyncSettingsStore
{
    public async Task<LeaderboardSyncPolicy> GetLeaderboardPolicyAsync(CancellationToken cancellationToken = default)
    {
        var row = await GetOrCreateLeaderboardSettingsAsync(cancellationToken);
        return LeaderboardSyncPolicy.Clamp(
            row.HotIntervalMinutes,
            row.ColdIntervalMinutes,
            row.HotActivityWindowHours);
    }

    public async Task<IReadOnlyList<SyncRecurringJob>> GetRecurringJobsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureRecurringJobsSeededAsync(cancellationToken);
        return await db.SyncRecurringJobs
            .AsNoTracking()
            .OrderBy(j => j.JobId)
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminSyncSettingsDto> GetAdminSettingsAsync(CancellationToken cancellationToken = default)
    {
        var policy = await GetLeaderboardPolicyAsync(cancellationToken);
        var jobs = await GetRecurringJobsAsync(cancellationToken);
        return ToDto(policy, jobs);
    }

    public async Task<AdminSyncSettingsDto> UpdateAsync(
        PatchAdminSyncSettingsRequest patch,
        CancellationToken cancellationToken = default)
    {
        if (patch.Leaderboard is { } leaderboard)
        {
            ValidateLeaderboard(leaderboard);
            var row = await GetOrCreateLeaderboardSettingsAsync(cancellationToken);
            row.HotIntervalMinutes = leaderboard.HotIntervalMinutes;
            row.ColdIntervalMinutes = leaderboard.ColdIntervalMinutes;
            row.HotActivityWindowHours = leaderboard.HotActivityWindowHours;
            row.UpdatedAt = DateTimeOffset.UtcNow;
        }

        if (patch.RecurringJobs is { Count: > 0 })
        {
            await EnsureRecurringJobsSeededAsync(cancellationToken);
            var byId = await db.SyncRecurringJobs.ToDictionaryAsync(j => j.JobId, cancellationToken);
            foreach (var item in patch.RecurringJobs)
            {
                if (!byId.TryGetValue(item.JobId, out var job))
                {
                    throw new SyncSettingsValidationException($"Unknown recurring job '{item.JobId}'.");
                }

                ApplyRecurringJobPatch(job, item);
                job.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return await GetAdminSettingsAsync(cancellationToken);
    }

    private async Task<SyncLeaderboardSettings> GetOrCreateLeaderboardSettingsAsync(CancellationToken cancellationToken)
    {
        var row = await db.SyncLeaderboardSettings
            .FirstOrDefaultAsync(s => s.Id == SyncLeaderboardSettings.SingletonId, cancellationToken);
        if (row is not null)
        {
            return row;
        }

        row = new SyncLeaderboardSettings
        {
            Id = SyncLeaderboardSettings.SingletonId,
            HotIntervalMinutes = 15,
            ColdIntervalMinutes = 1440,
            HotActivityWindowHours = 168,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.SyncLeaderboardSettings.Add(row);
        await db.SaveChangesAsync(cancellationToken);
        return row;
    }

    private async Task EnsureRecurringJobsSeededAsync(CancellationToken cancellationToken)
    {
        var existingIds = await db.SyncRecurringJobs
            .AsNoTracking()
            .Select(j => j.JobId)
            .ToListAsync(cancellationToken);
        var existing = existingIds.ToHashSet(StringComparer.Ordinal);

        var now = DateTimeOffset.UtcNow;
        var addedAny = false;
        foreach (var (jobId, displayName, intervalMinutes, intervalDays) in DefaultRecurringJobs)
        {
            if (existing.Contains(jobId))
            {
                continue;
            }

            db.SyncRecurringJobs.Add(new SyncRecurringJob
            {
                JobId = jobId,
                DisplayName = displayName,
                IntervalMinutes = intervalMinutes,
                IntervalDays = intervalDays,
                UpdatedAt = now
            });
            addedAny = true;
        }

        if (!addedAny)
        {
            return;
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsSyncRecurringJobUniqueViolation(ex))
        {
            DetachAddedSyncRecurringJobs();
        }
    }

    private static readonly (string JobId, string DisplayName, int? IntervalMinutes, int? IntervalDays)[] DefaultRecurringJobs =
    [
        (SyncRecurringJobIds.MemberActivity, "Member activity", 5, null),
        (SyncRecurringJobIds.LeaderboardDispatch, "Leaderboard dispatch", 5, null),
        (SyncRecurringJobIds.MemberRank, "Member RA rank", 60, null),
        (SyncRecurringJobIds.GameMetadata, "Game metadata", null, 7),
        (SyncRecurringJobIds.MemberAchievements, "Member achievements", 360, null),
        (SyncRecurringJobIds.DiscordNotificationDispatch, "Discord notification dispatch", 5, null),
        (SyncRecurringJobIds.GameOfTheWeek, "Game of the week", 5, null)
    ];

    private void DetachAddedSyncRecurringJobs()
    {
        foreach (var entry in db.ChangeTracker.Entries<SyncRecurringJob>()
                     .Where(e => e.State == EntityState.Added)
                     .ToList())
        {
            entry.State = EntityState.Detached;
        }
    }

    private static bool IsSyncRecurringJobUniqueViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;

    private static void ValidateLeaderboard(PatchLeaderboardSyncSettingsRequest leaderboard)
    {
        if (leaderboard.HotIntervalMinutes is < 1 or > 60 * 24
            || leaderboard.ColdIntervalMinutes is < 1 or > 60 * 24 * 7
            || leaderboard.HotActivityWindowHours < 1)
        {
            throw new SyncSettingsValidationException("Leaderboard sync intervals are out of allowed range.");
        }
    }

    private static void ApplyRecurringJobPatch(SyncRecurringJob job, PatchRecurringJobSettingsRequest item)
    {
        switch (job.JobId)
        {
            case SyncRecurringJobIds.MemberActivity:
                if (item.IntervalMinutes is not { } activityMinutes
                    || activityMinutes is < 1 or > 60)
                {
                    throw new SyncSettingsValidationException("Member activity interval must be between 1 and 60 minutes.");
                }

                job.IntervalMinutes = activityMinutes;
                break;
            case SyncRecurringJobIds.LeaderboardDispatch:
                if (item.IntervalMinutes is not { } dispatchMinutes
                    || dispatchMinutes is < 1 or > 60)
                {
                    throw new SyncSettingsValidationException("Leaderboard dispatch interval must be between 1 and 60 minutes.");
                }

                job.IntervalMinutes = dispatchMinutes;
                break;
            case SyncRecurringJobIds.MemberRank:
                if (item.IntervalMinutes is not { } rankMinutes
                    || rankMinutes is < 1 or > 60 * 24)
                {
                    throw new SyncSettingsValidationException("Member rank interval must be between 1 and 1440 minutes.");
                }

                job.IntervalMinutes = rankMinutes;
                break;
            case SyncRecurringJobIds.GameMetadata:
                if (item.IntervalDays is not { } metadataDays
                    || metadataDays is < 1 or > 365)
                {
                    throw new SyncSettingsValidationException("Game metadata interval must be between 1 and 365 days.");
                }

                job.IntervalDays = metadataDays;
                break;
            case SyncRecurringJobIds.MemberAchievements:
                if (item.IntervalMinutes is not { } achievementMinutes
                    || achievementMinutes is < 1 or > 60 * 24)
                {
                    throw new SyncSettingsValidationException("Member achievements interval must be between 1 and 1440 minutes.");
                }

                job.IntervalMinutes = achievementMinutes;
                break;
            case SyncRecurringJobIds.DiscordNotificationDispatch:
                if (item.IntervalMinutes is not { } discordMinutes
                    || discordMinutes is < 1 or > 60)
                {
                    throw new SyncSettingsValidationException("Discord notification dispatch interval must be between 1 and 60 minutes.");
                }

                job.IntervalMinutes = discordMinutes;
                break;
            case SyncRecurringJobIds.GameOfTheWeek:
                if (item.IntervalMinutes is not { } gotwMinutes
                    || gotwMinutes is < 1 or > 60)
                {
                    throw new SyncSettingsValidationException("Game of the week interval must be between 1 and 60 minutes.");
                }

                job.IntervalMinutes = gotwMinutes;
                break;
            default:
                throw new SyncSettingsValidationException($"Unknown recurring job '{job.JobId}'.");
        }
    }

    private static AdminSyncSettingsDto ToDto(LeaderboardSyncPolicy policy, IReadOnlyList<SyncRecurringJob> jobs)
        => new(
            new AdminLeaderboardSyncSettingsDto(
                policy.HotIntervalMinutes,
                policy.ColdIntervalMinutes,
                policy.HotActivityWindowHours),
            jobs
                .OrderBy(j => j.JobId)
                .Select(j => new AdminRecurringJobSettingsDto(
                    j.JobId,
                    j.DisplayName,
                    j.IntervalMinutes,
                    j.IntervalDays))
                .ToList());
}

public sealed class SyncSettingsValidationException(string message) : Exception(message);
