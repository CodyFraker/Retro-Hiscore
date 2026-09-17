namespace RetroHiscore.Api.Features.Sync;

public sealed record MemberRecentGamePlayChange(
    Guid MemberId,
    int RaGameId,
    DateTimeOffset LastPlayedAt,
    int NumAchieved,
    int NumPossibleAchievements,
    bool IsNewOrUpdated);
