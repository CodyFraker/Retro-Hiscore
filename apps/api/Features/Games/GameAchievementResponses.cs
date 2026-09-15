namespace RetroHiscore.Api.Features.Games;

public sealed record GameAchievementMemberDto(
    Guid MemberId,
    string RaUsername,
    string DisplayName,
    string? AvatarUrl);

public sealed record GameAchievementCatalogItemDto(
    int RaAchievementId,
    string Title,
    string? Description,
    int Points,
    int TrueRatio,
    string? BadgeUrl,
    int DisplayOrder);

public sealed record GameAchievementMemberUnlockDto(
    Guid MemberId,
    int RaAchievementId,
    DateTimeOffset? DateEarned,
    bool HardcoreEarned);

public sealed record GameAchievementMemberSummaryDto(
    Guid MemberId,
    int AchievementsEarned,
    int AchievementsTotal,
    int PointsEarned,
    int PointsPossible,
    DateTimeOffset? LastUnlockAt);

public sealed record GameAchievementRecentUnlockDto(
    Guid MemberId,
    string RaUsername,
    string DisplayName,
    string? AvatarUrl,
    int RaAchievementId,
    string Title,
    int Points,
    string? BadgeUrl,
    DateTimeOffset? DateEarned,
    bool HardcoreEarned);

public sealed record GameAchievementsResponse(
    int RaGameId,
    string Title,
    DateTimeOffset? AchievementProgressSyncedAt,
    IReadOnlyList<GameAchievementCatalogItemDto> Achievements,
    IReadOnlyList<GameAchievementMemberDto> Members,
    IReadOnlyList<GameAchievementMemberUnlockDto> Unlocks,
    IReadOnlyList<GameAchievementMemberSummaryDto> MemberSummaries,
    GameAchievementMemberSummaryDto? CompletionLeader,
    int MembersMastered,
    IReadOnlyList<GameAchievementRecentUnlockDto> RecentUnlocks);

public sealed record AchievementDistributionBucketDto(
    int AchievementsEarned,
    int PlayerCount);

public sealed record GameAchievementDistributionResponse(
    int RaGameId,
    bool Available,
    DateTimeOffset? SyncedAt,
    IReadOnlyList<AchievementDistributionBucketDto> SoftcoreBuckets,
    IReadOnlyList<AchievementDistributionBucketDto> HardcoreBuckets);
