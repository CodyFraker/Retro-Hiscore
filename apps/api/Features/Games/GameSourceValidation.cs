using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Games;

public static class GameSourceValidation
{
    public static Dictionary<string, string[]>? ValidateUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return new Dictionary<string, string[]>
            {
                [nameof(url)] = ["url is required."]
            };
        }

        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps)
        {
            return new Dictionary<string, string[]>
            {
                [nameof(url)] = ["url must be a valid https URL."]
            };
        }

        return null;
    }

    public static Dictionary<string, string[]>? ValidateSourceType(string? sourceType, out GameSourceType parsed)
    {
        parsed = default;
        if (string.IsNullOrWhiteSpace(sourceType))
        {
            return new Dictionary<string, string[]>
            {
                [nameof(sourceType)] = ["sourceType is required."]
            };
        }

        if (!Enum.TryParse(sourceType, ignoreCase: true, out parsed)
            || !Enum.IsDefined(parsed))
        {
            return new Dictionary<string, string[]>
            {
                [nameof(sourceType)] = ["sourceType must be GoogleDrive, Mega, Mediafire, or Other."]
            };
        }

        return null;
    }

    public static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length > maxLength ? trimmed[..maxLength] : trimmed;
    }
}
