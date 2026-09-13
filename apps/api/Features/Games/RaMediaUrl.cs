using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Games;

public static class RaMediaUrl
{
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
