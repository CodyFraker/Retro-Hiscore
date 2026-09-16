using System.Text.Json;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Notifications;

public static class DiscordWebhookSamplePayloads
{
    public static string BuildJson(DiscordNotificationEventKind kind)
    {
        var values = DiscordTokenCatalog.SampleValues(kind);
        return kind switch
        {
            DiscordNotificationEventKind.GameTracked => JsonSerializer.Serialize(
                new GameTrackedNotificationPayload(
                    int.Parse(values["gid"]),
                    values["gt"],
                    values["cn"],
                    int.Parse(values["nl"]),
                    int.Parse(values["na"]),
                    int.Parse(values["te"]),
                    DateTimeOffset.UtcNow),
                NotificationPayloadJson.Options),
            DiscordNotificationEventKind.AchievementUnlocked => JsonSerializer.Serialize(
                new AchievementUnlockedNotificationPayload(
                    Guid.Empty,
                    values["run"],
                    values["usr"],
                    int.Parse(values["gid"]),
                    values["gt"],
                    1,
                    values["ach"],
                    int.Parse(values["ap"]),
                    values["hc"] == "1",
                    DateTimeOffset.UtcNow),
                NotificationPayloadJson.Options),
            _ => JsonSerializer.Serialize(
                new LeaderboardNotificationPayload(
                    Guid.Empty,
                    values["run"],
                    values["usr"],
                    int.Parse(values["gid"]),
                    values["gt"],
                    long.Parse(values["lid"]),
                    values["lb"],
                    long.TryParse(values["sd"], out var sd) ? sd : null,
                    values["sc"],
                    int.TryParse(values["fr"], out var fr) ? fr : null,
                    int.TryParse(values["fd"], out var fd) ? fd : null,
                    int.TryParse(values["gr"], out var gr) ? gr : null,
                    int.TryParse(values["ge"], out var ge) ? ge : null,
                    int.TryParse(values["gd"], out var gd) ? gd : null),
                NotificationPayloadJson.Options)
        };
    }
}
