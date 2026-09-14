namespace RetroHiscore.Api.Domain;

public sealed class SyncRecurringJob
{
    public string JobId { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public int? IntervalMinutes { get; set; }
    public int? IntervalDays { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
