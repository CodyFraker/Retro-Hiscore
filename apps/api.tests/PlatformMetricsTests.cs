using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Tests;

public sealed class PlatformMetricsTests
{
    [Fact]
    public void RecordSyncRun_WhenFinishedAtIsNull_DoesNotThrow()
    {
        var run = new SyncRun
        {
            Kind = SyncKind.MemberActivity,
            Trigger = SyncTrigger.Scheduled,
            Status = SyncRunStatus.Running,
            StartedAt = DateTimeOffset.UtcNow
        };

        SyncRunMetrics.Record(run);
    }

    [Fact]
    public void RecordSyncRun_WhenFinishedAtIsSet_DoesNotThrow()
    {
        var started = DateTimeOffset.UtcNow.AddMinutes(-1);
        var run = new SyncRun
        {
            Kind = SyncKind.MemberActivity,
            Trigger = SyncTrigger.Manual,
            Status = SyncRunStatus.Succeeded,
            StartedAt = started,
            FinishedAt = DateTimeOffset.UtcNow
        };

        SyncRunMetrics.Record(run);
    }
}
