namespace RetroHiscore.Api.Features.Admin;

public sealed record AdminSyncSettingsDto(
    AdminLeaderboardSyncSettingsDto Leaderboard,
    IReadOnlyList<AdminRecurringJobSettingsDto> RecurringJobs);

public sealed record AdminLeaderboardSyncSettingsDto(
    int HotIntervalMinutes,
    int ColdIntervalMinutes,
    int HotActivityWindowHours);

public sealed record AdminRecurringJobSettingsDto(
    string JobId,
    string DisplayName,
    int? IntervalMinutes,
    int? IntervalDays);

public sealed record PatchAdminSyncSettingsRequest(
    PatchLeaderboardSyncSettingsRequest? Leaderboard,
    IReadOnlyList<PatchRecurringJobSettingsRequest>? RecurringJobs);

public sealed record PatchLeaderboardSyncSettingsRequest(
    int HotIntervalMinutes,
    int ColdIntervalMinutes,
    int HotActivityWindowHours);

public sealed record PatchRecurringJobSettingsRequest(
    string JobId,
    int? IntervalMinutes,
    int? IntervalDays);
