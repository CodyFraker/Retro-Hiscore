using System.Globalization;

namespace RetroHiscore.Api.Features.Ra;

public static class RaDateTime
{
    public static DateTimeOffset? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var parsed))
        {
            return parsed.ToUniversalTime();
        }

        return null;
    }
}
