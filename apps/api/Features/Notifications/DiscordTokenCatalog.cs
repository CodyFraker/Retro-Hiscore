using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Notifications;

public sealed record DiscordTokenDefinition(string Code, string Label);

public static class DiscordTokenCatalog
{
    private static readonly DiscordTokenDefinition[] LeaderboardTokens =
    [
        new("usr", "Display name"),
        new("run", "RA username"),
        new("gt", "Game title"),
        new("gid", "RA game id"),
        new("lb", "Leaderboard title"),
        new("lid", "RA leaderboard id"),
        new("sc", "Formatted score"),
        new("sd", "Score delta"),
        new("fr", "Friend rank"),
        new("fd", "Friend rank delta"),
        new("gr", "Global rank"),
        new("ge", "Global entry count"),
        new("gd", "Global rank delta")
    ];

    private static readonly DiscordTokenDefinition[] GameTrackedTokens =
    [
        new("gt", "Game title"),
        new("gid", "RA game id"),
        new("cn", "Console name"),
        new("nl", "Leaderboard count"),
        new("na", "Achievement count"),
        new("te", "Total LB entries")
    ];

    private static readonly DiscordTokenDefinition[] AchievementTokens =
    [
        new("usr", "Display name"),
        new("run", "RA username"),
        new("gt", "Game title"),
        new("gid", "RA game id"),
        new("ach", "Achievement title"),
        new("ap", "Achievement points"),
        new("hc", "Hardcore (1/0)")
    ];

    public static IReadOnlyList<DiscordTokenDefinition> ForKind(DiscordNotificationEventKind kind)
        => kind switch
        {
            DiscordNotificationEventKind.GameTracked => GameTrackedTokens,
            DiscordNotificationEventKind.AchievementUnlocked => AchievementTokens,
            _ => LeaderboardTokens
        };

    public static IReadOnlySet<string> AllowedCodesForKinds(IEnumerable<DiscordNotificationEventKind> kinds)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var kind in kinds)
        {
            foreach (var token in ForKind(kind))
            {
                set.Add(token.Code);
            }
        }

        return set;
    }

    public static IReadOnlyDictionary<string, string> BuildValues(
        DiscordNotificationEventKind kind,
        string payloadJson)
    {
        return kind switch
        {
            DiscordNotificationEventKind.GameTracked => BuildGameTracked(
                System.Text.Json.JsonSerializer.Deserialize<GameTrackedNotificationPayload>(
                    payloadJson,
                    NotificationPayloadJson.Options)!),
            DiscordNotificationEventKind.AchievementUnlocked => BuildAchievement(
                System.Text.Json.JsonSerializer.Deserialize<AchievementUnlockedNotificationPayload>(
                    payloadJson,
                    NotificationPayloadJson.Options)!),
            _ => BuildLeaderboard(
                System.Text.Json.JsonSerializer.Deserialize<LeaderboardNotificationPayload>(
                    payloadJson,
                    NotificationPayloadJson.Options)!)
        };
    }

    private static IReadOnlyDictionary<string, string> BuildLeaderboard(LeaderboardNotificationPayload p)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["usr"] = p.DisplayName,
            ["run"] = p.RaUsername,
            ["gt"] = p.GameTitle,
            ["gid"] = p.RaGameId.ToString(),
            ["lb"] = p.LeaderboardTitle,
            ["lid"] = p.RaLeaderboardId.ToString(),
            ["sc"] = p.FormattedScore ?? "",
            ["sd"] = p.ScoreDelta?.ToString() ?? "",
            ["fr"] = p.FriendRank?.ToString() ?? "",
            ["fd"] = p.FriendRankDelta?.ToString() ?? "",
            ["gr"] = p.GlobalRank?.ToString() ?? "",
            ["ge"] = p.GlobalEntryCount?.ToString() ?? "",
            ["gd"] = p.GlobalRankDelta?.ToString() ?? ""
        };

    private static IReadOnlyDictionary<string, string> BuildGameTracked(GameTrackedNotificationPayload p)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["gt"] = p.GameTitle,
            ["gid"] = p.RaGameId.ToString(),
            ["cn"] = p.ConsoleName ?? "",
            ["nl"] = p.NumLeaderboards.ToString(),
            ["na"] = p.NumAchievements.ToString(),
            ["te"] = p.TotalLbEntries.ToString()
        };

    private static IReadOnlyDictionary<string, string> BuildAchievement(AchievementUnlockedNotificationPayload p)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["usr"] = p.DisplayName,
            ["run"] = p.RaUsername,
            ["gt"] = p.GameTitle,
            ["gid"] = p.RaGameId.ToString(),
            ["ach"] = p.AchievementTitle,
            ["ap"] = p.AchievementPoints.ToString(),
            ["hc"] = p.Hardcore ? "1" : "0"
        };

    public static IReadOnlyDictionary<string, string> SampleValues(DiscordNotificationEventKind kind)
        => kind switch
        {
            DiscordNotificationEventKind.GameTracked => BuildGameTracked(new GameTrackedNotificationPayload(
                38130, "Pinball", "Arcade", 12, 50, 5000, DateTimeOffset.UtcNow)),
            DiscordNotificationEventKind.AchievementUnlocked => BuildAchievement(new AchievementUnlockedNotificationPayload(
                Guid.Empty, "sample", "Sample Player", 38130, "Pinball", 1, "First Flip", 5, true, DateTimeOffset.UtcNow)),
            DiscordNotificationEventKind.LeaderboardNewSubmission => BuildLeaderboard(new LeaderboardNotificationPayload(
                Guid.Empty, "sample", "Sample Player", 38130, "Pinball", 3813001, "High Score", 100, "1,500", 3, null, 120, 5000, null)),
            _ => BuildLeaderboard(new LeaderboardNotificationPayload(
                Guid.Empty, "sample", "Sample Player", 38130, "Pinball", 3813001, "High Score", 50, "1,550", 2, 1, 100, 5000, 5))
        };
}
