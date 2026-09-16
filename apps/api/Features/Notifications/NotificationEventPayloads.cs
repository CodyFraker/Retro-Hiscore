using System.Text.Json.Serialization;

namespace RetroHiscore.Api.Features.Notifications;

public sealed record LeaderboardNotificationPayload(
    Guid MemberId,
    string RaUsername,
    string DisplayName,
    [property: JsonPropertyName("raGameId")] int RaGameId,
    string GameTitle,
    long RaLeaderboardId,
    string LeaderboardTitle,
    long? ScoreDelta,
    string? FormattedScore,
    int? FriendRank,
    int? FriendRankDelta,
    int? GlobalRank,
    int? GlobalEntryCount,
    int? GlobalRankDelta);

public sealed record GameTrackedPendingNotificationPayload(
    [property: JsonPropertyName("raGameId")] int RaGameId);

public sealed record GameTrackedNotificationPayload(
    [property: JsonPropertyName("raGameId")] int RaGameId,
    string GameTitle,
    string? ConsoleName,
    int NumLeaderboards,
    int NumAchievements,
    int TotalLbEntries,
    DateTimeOffset TrackedAt);

public sealed record AchievementUnlockedNotificationPayload(
    Guid MemberId,
    string RaUsername,
    string DisplayName,
    [property: JsonPropertyName("raGameId")] int RaGameId,
    string GameTitle,
    int RaAchievementId,
    string AchievementTitle,
    int AchievementPoints,
    bool Hardcore,
    DateTimeOffset EarnedAt);
