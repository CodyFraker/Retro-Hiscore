namespace RetroHiscore.Api.Options;

public class SyncOptions
{
    public const string SectionName = "Sync";

    public int IntervalMinutes { get; set; } = 15;
    public int ManualCooldownSeconds { get; set; } = 60;
    public int RecentGamesPerMember { get; set; } = 15;
    public int DashboardRecentGamesLimit { get; set; } = 15;
    public int MaxLeaderboardEligibilityProbesPerSync { get; set; } = 10;
}

public class GameMetadataSyncOptions
{
    public const string SectionName = "GameMetadataSync";

    public int ManualCooldownSeconds { get; set; } = 300;
}

public class ConsoleIconSyncOptions
{
    public const string SectionName = "ConsoleIconSync";

    public int ManualCooldownSeconds { get; set; } = 300;
    public bool ForceRefresh { get; set; }
}

public class HangfireDashboardOptions
{
    public const string SectionName = "Hangfire";

    public string DashboardPath { get; set; } = "/hangfire";
    public string? DashboardSecret { get; set; }
}

public class ScalarDashboardOptions
{
    public const string SectionName = "Scalar";

    public string? DashboardSecret { get; set; }
}

public class NotificationOptions
{
    public const string SectionName = "Notifications";

    public string? DiscordWebhookUrl { get; set; }
    public bool NotifyOnRankChange { get; set; } = true;
    public bool NotifyOnNewLead { get; set; } = true;
}
