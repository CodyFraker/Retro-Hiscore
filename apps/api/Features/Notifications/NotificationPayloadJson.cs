using System.Text.Json;

namespace RetroHiscore.Api.Features.Notifications;

public static class NotificationPayloadJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}
