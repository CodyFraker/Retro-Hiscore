using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Games;

public static class RaMediaUrl
{
    public static string SiteBaseUrlFromApiBase(string apiBaseUrl)
    {
        var trimmed = apiBaseUrl.TrimEnd('/');
        if (trimmed.EndsWith("/API", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed[..^4];
        }

        return trimmed;
    }

    public static string? ToBadgeUrl(string? badgeName, string mediaBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(badgeName))
        {
            return null;
        }

        var baseUrl = mediaBaseUrl.TrimEnd('/');
        return $"{baseUrl}/Badge/{badgeName}.png";
    }

    public static string? ToAbsolute(string? relativePath, string mediaBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        var baseUrl = mediaBaseUrl.TrimEnd('/');
        var path = relativePath.StartsWith('/') ? relativePath : $"/{relativePath}";
        return $"{baseUrl}{path}";
    }

    public static GameImageUrls FromGame(Game game, string mediaBaseUrl)
        => new(
            ToAbsolute(game.ImageBoxArt, mediaBaseUrl),
            ToAbsolute(game.ImageIcon, mediaBaseUrl),
            ToAbsolute(game.ImageTitle, mediaBaseUrl),
            ToAbsolute(game.ImageIngame, mediaBaseUrl));
}

public sealed record GameImageUrls(
    string? ImageBoxArtUrl,
    string? ImageIconUrl,
    string? ImageTitleUrl,
    string? ImageIngameUrl);
