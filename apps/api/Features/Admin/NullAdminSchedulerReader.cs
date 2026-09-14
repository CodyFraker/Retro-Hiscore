namespace RetroHiscore.Api.Features.Admin;

public sealed class NullAdminSchedulerReader : IAdminSchedulerReader
{
    public IReadOnlyList<RecurringJobSnapshotDto> GetRecurringJobs() => [];
}
