namespace RetroHiscore.Api.Features.Sync;

public static class SyncRecurringJobIds
{
    public const string MemberActivity = "ra-member-activity-sync";
    public const string LeaderboardDispatch = "ra-leaderboard-dispatch";
    public const string MemberRank = "ra-member-rank-sync";
    public const string GameMetadata = "ra-game-metadata-sync";
    public const string MemberAchievements = "ra-member-achievements-sync";

    public static readonly string[] All =
    [
        MemberActivity,
        LeaderboardDispatch,
        MemberRank,
        GameMetadata,
        MemberAchievements
    ];
}
