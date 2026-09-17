using Hangfire;
using Hangfire.Storage;
using RetroHiscore.Api.Features.Sync;

namespace RetroHiscore.Api.Features.Admin;

public sealed class HangfireAdminSchedulerReader : IAdminSchedulerReader
{

    public IReadOnlyList<RecurringJobSnapshotDto> GetRecurringJobs()
    {
        var monitoring = JobStorage.Current.GetMonitoringApi();
        var recurring = JobStorage.Current.GetConnection().GetRecurringJobs();
        var byId = recurring.ToDictionary(j => j.Id, StringComparer.Ordinal);

        var results = new List<RecurringJobSnapshotDto>();
        foreach (var jobId in SyncRecurringJobIds.All)
        {
            if (!byId.TryGetValue(jobId, out var job))
            {
                results.Add(new RecurringJobSnapshotDto(jobId, null, null, null, null, null, null));
                continue;
            }

            DateTime? lastExecution = null;
            string? lastState = null;
            if (!string.IsNullOrEmpty(job.LastJobId))
            {
                var details = monitoring.JobDetails(job.LastJobId);
                if (details is not null)
                {
                    lastState = details.History?.FirstOrDefault()?.StateName;
                    lastExecution = details.CreatedAt;
                }
            }

            results.Add(new RecurringJobSnapshotDto(
                jobId,
                job.Cron,
                lastExecution,
                job.NextExecution,
                lastState,
                null,
                null));
        }

        return results;
    }
}
