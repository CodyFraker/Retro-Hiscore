using Hangfire;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Sync;

public sealed class RecurringSyncJobRegistrar(IRecurringJobManager recurringJobs, AppDbContext db) : IRecurringSyncJobRegistrar
{
    public async Task RegisterAllAsync(CancellationToken cancellationToken = default)
    {
        recurringJobs.RemoveIfExists("ra-leaderboard-sync");

        var jobs = await db.SyncRecurringJobs.AsNoTracking().ToListAsync(cancellationToken);
        foreach (var job in jobs)
        {
            Register(job);
        }
    }

    private void Register(SyncRecurringJob job)
    {
        switch (job.JobId)
        {
            case SyncRecurringJobIds.MemberActivity:
                recurringJobs.AddOrUpdate<MemberActivitySyncJob>(
                    job.JobId,
                    x => x.RunScheduledAsync(CancellationToken.None),
                    SyncRecurringJobCron.ForMinuteInterval(job.IntervalMinutes ?? 15, 1, 60));
                break;
            case SyncRecurringJobIds.LeaderboardDispatch:
                recurringJobs.AddOrUpdate<LeaderboardSyncDispatchJob>(
                    job.JobId,
                    x => x.RunScheduledAsync(CancellationToken.None),
                    SyncRecurringJobCron.ForMinuteInterval(job.IntervalMinutes ?? 5, 1, 60));
                break;
            case SyncRecurringJobIds.MemberRank:
                recurringJobs.AddOrUpdate<MemberRankSyncJob>(
                    job.JobId,
                    x => x.RunScheduledAsync(CancellationToken.None),
                    SyncRecurringJobCron.ForMinuteInterval(job.IntervalMinutes ?? 60, 1, 60 * 24));
                break;
            case SyncRecurringJobIds.GameMetadata:
                recurringJobs.AddOrUpdate<GameMetadataSyncJob>(
                    job.JobId,
                    x => x.RunScheduledAsync(CancellationToken.None),
                    SyncRecurringJobCron.ForIntervalDays(job.IntervalDays ?? 7));
                break;
            case SyncRecurringJobIds.MemberAchievements:
                recurringJobs.AddOrUpdate<MemberAchievementSyncJob>(
                    job.JobId,
                    x => x.RunScheduledAsync(CancellationToken.None),
                    SyncRecurringJobCron.ForMinuteInterval(job.IntervalMinutes ?? 360, 1, 60 * 24));
                break;
            default:
                throw new InvalidOperationException($"Unsupported recurring job id '{job.JobId}'.");
        }
    }
}

public sealed class NullRecurringSyncJobRegistrar : IRecurringSyncJobRegistrar
{
    public Task RegisterAllAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
