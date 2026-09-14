namespace RetroHiscore.Api.Features.Admin;

public interface IAdminSchedulerReader
{
    IReadOnlyList<RecurringJobSnapshotDto> GetRecurringJobs();
}

public sealed record RecurringJobSnapshotDto(
    string JobId,
    string? Cron,
    DateTime? LastExecution,
    DateTime? NextExecution,
    string? LastJobState,
    int? ConfiguredIntervalMinutes,
    int? ConfiguredIntervalDays);
