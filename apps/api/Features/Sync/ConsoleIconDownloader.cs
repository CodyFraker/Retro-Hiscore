namespace RetroHiscore.Api.Features.Sync;

public interface IConsoleIconDownloader
{
    Task<byte[]> DownloadAsync(string iconUrl, CancellationToken cancellationToken = default);
}

public sealed class ConsoleIconDownloader(HttpClient httpClient) : IConsoleIconDownloader
{
    public async Task<byte[]> DownloadAsync(string iconUrl, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(iconUrl, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
