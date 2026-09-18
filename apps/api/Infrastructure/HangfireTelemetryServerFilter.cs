using System.Diagnostics;
using Hangfire.Server;

namespace RetroHiscore.Api.Infrastructure;

public sealed class HangfireTelemetryServerFilter : IServerFilter
{
    public void OnPerforming(PerformingContext context)
    {
        context.Items["TelemetryStartedAt"] = Stopwatch.GetTimestamp();
    }

    public void OnPerformed(PerformedContext context)
    {
        if (!context.Items.TryGetValue("TelemetryStartedAt", out var startedObj)
            || startedObj is not long startedAt)
        {
            return;
        }

        var durationMs = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
        var jobName = ResolveJobName(context);
        var status = context.Exception is null ? "succeeded" : "failed";
        PlatformMetrics.RecordHangfireJob(jobName, status, durationMs);
    }

    private static string ResolveJobName(PerformedContext context)
    {
        var typeName = context.BackgroundJob.Job.Type.Name;
        var methodName = context.BackgroundJob.Job.Method.Name;
        return $"{typeName}.{methodName}";
    }
}
