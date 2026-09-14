using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Features.Dashboard;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.Notifications;

public interface IDiscordNotificationService
{
    Task NotifyActivityAsync(IReadOnlyList<ActivityItemDto> activity, CancellationToken cancellationToken = default);
}

public sealed class DiscordNotificationService(
    IHttpClientFactory httpClientFactory,
    IOptions<NotificationOptions> options,
    ILogger<DiscordNotificationService> logger) : IDiscordNotificationService
{
    private readonly NotificationOptions _options = options.Value;

    public async Task NotifyActivityAsync(IReadOnlyList<ActivityItemDto> activity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.DiscordWebhookUrl))
        {
            return;
        }

        var items = activity.Where(ShouldNotify).ToList();
        if (items.Count == 0)
        {
            return;
        }

        var lines = items.Select(FormatLine).ToList();
        var payload = new DiscordWebhookPayload
        {
            Content = string.Join("\n", lines.Take(10))
        };

        try
        {
            var client = httpClientFactory.CreateClient(nameof(DiscordNotificationService));
            var response = await client.PostAsJsonAsync(_options.DiscordWebhookUrl, payload, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Discord webhook returned {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send Discord notification");
        }
    }

    private bool ShouldNotify(ActivityItemDto item)
    {
        if (item.FriendRankDelta is null or 0)
        {
            return false;
        }

        if (_options.NotifyOnRankChange)
        {
            return true;
        }

        return _options.NotifyOnNewLead && item.FriendRankDelta > 0;
    }

    private static string FormatLine(ActivityItemDto item)
    {
        var rankPart = item.FriendRankDelta switch
        {
            > 0 => $" moved up {item.FriendRankDelta} friend rank(s)",
            < 0 => $" dropped {Math.Abs(item.FriendRankDelta.Value)} friend rank(s)",
            _ => ""
        };

        var globalPart = "";
        if (item.GlobalRank is not null && item.GlobalEntryCount is not null)
        {
            globalPart = $" (now #{item.GlobalRank} of {item.GlobalEntryCount:N0})";
        }
        else if (item.GlobalRank is not null)
        {
            globalPart = $" (now #{item.GlobalRank} globally)";
        }

        if (item.GlobalRankDelta is not null && item.GlobalRankDelta != 0)
        {
            var direction = item.GlobalRankDelta > 0 ? "improved" : "dropped";
            globalPart += $" — global {direction} {Math.Abs(item.GlobalRankDelta.Value)}";
        }

        return $"**{item.DisplayName}**{rankPart} on **{item.GameTitle}** — {item.LeaderboardTitle}{globalPart}";
    }

    private sealed class DiscordWebhookPayload
    {
        [JsonPropertyName("content")]
        public string Content { get; set; } = "";
    }
}
