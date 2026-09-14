using Hangfire;
using Hangfire.Storage;

namespace RetroHiscore.Api.Features.Admin;

public sealed class HangfireAdminSchedulerReader : IAdminSchedulerReader
{
    private static readonly string[] TrackedJobIds = ["ra-leaderboard-sync", "ra-game-metadata-sync"];

    public IReadOnlyList<RecurringJobSnapshotDto> GetRecurringJobs()
    {
        var monitoring = JobStorage.Current.GetMonitoringApi();
        var recurring = JobStorage.Current.GetConnection().GetRecurringJobs();
        var byId = recurring.ToDictionary(j => j.Id, StringComparer.Ordinal);

        var results = new List<RecurringJobSnapshotDto>();
        foreach (var jobId in TrackedJobIds)
        {
            if (!byId.TryGetValue(jobId, out var job))
            {
                results.Add(new RecurringJobSnapshotDto(jobId, null, null, null, null));
                continue;
            }

            DateTime? lastExecution = null;
            string? lastState = null;
            if (!string.IsNullOrEmpty(job.LastJobId))
            {
                var details = monitoring.JobDetails(job.LastJobId);
                lastState = details.History.FirstOrDefault()?.StateName;
                lastExecution = details.CreatedAt;
            }

            results.Add(new RecurringJobSnapshotDto(
                jobId,
                job.Cron,
                lastExecution,
                job.NextExecution,
                lastState));
        }

        return results;
    }
}
