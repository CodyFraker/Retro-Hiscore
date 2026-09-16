using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Notifications;

public static class DiscordWebhookDefaultTemplates
{
    public static string ForEventKind(DiscordNotificationEventKind kind)
        => kind switch
        {
            DiscordNotificationEventKind.GameTracked => """
                {
                  "embeds": [{
                    "title": "{{gt}}",
                    "description": "Now tracked on Retro-Hiscore!\n{{nl}} leaderboards · {{na}} achievements · {{te}} total entries",
                    "color": 5793266
                  }]
                }
                """,
            DiscordNotificationEventKind.LeaderboardFriendOvertake => """
                {
                  "embeds": [{
                    "title": "{{usr}}",
                    "description": "Moved up **{{fd}}** on **{{gt}}** — {{lb}}\nScore **{{sc}}** · friend #{{fr}}",
                    "color": 3066993
                  }]
                }
                """,
            DiscordNotificationEventKind.LeaderboardNewSubmission => """
                {
                  "embeds": [{
                    "title": "{{usr}}",
                    "description": "Posted on **{{gt}}** — {{lb}}\n**{{sc}}** · friend #{{fr}}",
                    "color": 3447003
                  }]
                }
                """,
            DiscordNotificationEventKind.AchievementUnlocked => """
                {
                  "embeds": [{
                    "title": "{{usr}}",
                    "description": "Unlocked **{{ach}}** ({{ap}} pts) in **{{gt}}**",
                    "color": 15844367
                  }]
                }
                """,
            _ => """{ "embeds": [{ "title": "Notification", "description": "Event" }] }"""
        };
}
