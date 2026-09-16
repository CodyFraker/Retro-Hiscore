namespace RetroHiscore.Api.Domain;

public enum DiscordNotificationEventKind
{
    GameTracked = 0,
    LeaderboardFriendOvertake = 1,
    LeaderboardNewSubmission = 2,
    AchievementUnlocked = 3
}
