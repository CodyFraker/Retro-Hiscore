using System.Globalization;
using System.Text.Json;

namespace RetroHiscore.Api.Features.Ra;

public static class RaUserSummaryFormatting
{
    public static string? FormatStatus(JsonElement status)
    {
        if (status.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        if (status.ValueKind == JsonValueKind.String)
        {
            return status.GetString();
        }

        if (status.ValueKind == JsonValueKind.Object
            && status.TryGetProperty("Status", out var inner)
            && inner.ValueKind == JsonValueKind.String)
        {
            return inner.GetString();
        }

        return null;
    }

    public static DateTimeOffset? ParseDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    public static bool IsOnlineOrPlaying(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return false;
        }

        var normalized = status.ToLowerInvariant();
        return normalized == "online" || normalized.Contains("playing");
    }
}
