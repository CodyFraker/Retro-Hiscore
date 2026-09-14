using Hangfire;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Sync;

public sealed class RecurringSyncJobRegistrar(AppDbContext db) : IRecurringSyncJobRegistrar
{
    public async Task RegisterAllAsync(CancellationToken cancellationToken = default)
    {
        var jobs = await db.SyncRecurringJobs.AsNoTracking().ToListAsync(cancellationToken);
        foreach (var job in jobs)
        {
            Register(job);
        }
    }

    private static void Register(SyncRecurringJob job)
    {
        switch (job.JobId)
        {
            case SyncRecurringJobIds.MemberActivity:
                RecurringJob.AddOrUpdate<MemberActivitySyncJob>(
                    job.JobId,
                    x => x.RunScheduledAsync(CancellationToken.None),
                    SyncRecurringJobCron.ForMinuteInterval(job.IntervalMinutes ?? 15, 1, 60));
                break;
            case SyncRecurringJobIds.LeaderboardDispatch:
                RecurringJob.AddOrUpdate<LeaderboardSyncDispatchJob>(
                    job.JobId,
                    x => x.RunScheduledAsync(CancellationToken.None),
                    SyncRecurringJobCron.ForMinuteInterval(job.IntervalMinutes ?? 5, 1, 60));
                break;
            case SyncRecurringJobIds.MemberRank:
                RecurringJob.AddOrUpdate<MemberRankSyncJob>(
                    job.JobId,
                    x => x.RunScheduledAsync(CancellationToken.None),
                    SyncRecurringJobCron.ForMinuteInterval(job.IntervalMinutes ?? 60, 1, 60 * 24));
                break;
            case SyncRecurringJobIds.GameMetadata:
                RecurringJob.AddOrUpdate<GameMetadataSyncJob>(
                    job.JobId,
                    x => x.RunScheduledAsync(CancellationToken.None),
                    SyncRecurringJobCron.ForIntervalDays(job.IntervalDays ?? 7));
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
