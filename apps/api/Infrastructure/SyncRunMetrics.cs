using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Infrastructure;

public static class SyncRunMetrics
{
    public static void Record(SyncRun run) => PlatformMetrics.RecordSyncRun(run);
}
