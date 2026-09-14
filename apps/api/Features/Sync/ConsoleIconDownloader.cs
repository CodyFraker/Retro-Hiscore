namespace RetroHiscore.Api.Features.Sync;

public sealed record ConsoleIconDownload(byte[] Data, string ContentType);

public interface IConsoleIconDownloader
{
    Task<ConsoleIconDownload> DownloadAsync(string iconUrl, CancellationToken cancellationToken = default);
}

public sealed class ConsoleIconDownloader(HttpClient httpClient) : IConsoleIconDownloader
{
    public async Task<ConsoleIconDownload> DownloadAsync(string iconUrl, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(iconUrl, cancellationToken);
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType;
        if (string.IsNullOrWhiteSpace(contentType))
        {
            contentType = "image/png";
        }

        return new ConsoleIconDownload(data, contentType);
    }
}
