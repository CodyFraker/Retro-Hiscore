using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace RetroHiscore.Api.Features.Ra;

public sealed class RaApiClient(HttpClient httpClient, IOptions<RaOptions> options, ILogger<RaApiClient> logger) : IRaApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly RaOptions _options = options.Value;

    public async Task<RaGameDto> GetGameAsync(int gameId, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var query = new Dictionary<string, string>
        {
            ["y"] = _options.ApiKey,
            ["i"] = gameId.ToString()
        };
        return await GetWithRetryAsync<RaGameDto>("API_GetGame.php", query, cancellationToken);
    }

    public async Task<IReadOnlyList<RaConsoleIdDto>> GetConsoleIdsAsync(CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var query = new Dictionary<string, string>
        {
            ["y"] = _options.ApiKey,
            ["g"] = "1"
        };
        var results = await GetWithRetryAsync<List<RaConsoleIdDto>>("API_GetConsoleIDs.php", query, cancellationToken);
        return results;
    }

    public Task<IReadOnlyList<RaGameLeaderboardDto>> GetGameLeaderboardsAsync(int gameId, CancellationToken cancellationToken = default)
        => GetAllPagesAsync<RaGameLeaderboardDto>(
            "API_GetGameLeaderboards.php",
            new Dictionary<string, string> { ["i"] = gameId.ToString() },
            cancellationToken);

    public async Task<IReadOnlyList<RaUserGameLeaderboardDto>> GetUserGameLeaderboardsAsync(
        int gameId,
        string usernameOrUlid,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAllPagesAsync<RaUserGameLeaderboardDto>(
                "API_GetUserGameLeaderboards.php",
                new Dictionary<string, string>
                {
                    ["i"] = gameId.ToString(),
                    ["u"] = usernameOrUlid
                },
                cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            logger.LogDebug(
                "RA returned 422 for user game leaderboards (no entries) user={User} game={GameId}",
                usernameOrUlid,
                gameId);
            return [];
        }
    }

    private async Task<IReadOnlyList<T>> GetAllPagesAsync<T>(
        string endpoint,
        Dictionary<string, string> query,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();

        var results = new List<T>();
        var offset = 0;
        const int pageSize = 500;

        while (true)
        {
            var pageQuery = new Dictionary<string, string>(query)
            {
                ["y"] = _options.ApiKey,
                ["c"] = pageSize.ToString(),
                ["o"] = offset.ToString()
            };

            var page = await GetWithRetryAsync<RaPagedResponse<T>>(endpoint, pageQuery, cancellationToken);
            if (page.Results.Count == 0)
            {
                break;
            }

            results.AddRange(page.Results);
            offset += page.Results.Count;

            if (results.Count >= page.Total || page.Results.Count < pageSize)
            {
                break;
            }
        }

        return results;
    }

    private async Task<T> GetWithRetryAsync<T>(string endpoint, Dictionary<string, string> query, CancellationToken cancellationToken)
    {
        const int maxAttempts = 5;
        var delay = TimeSpan.FromSeconds(2);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var url = BuildUrl(endpoint, query);
            using var response = await httpClient.GetAsync(url, cancellationToken);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                logger.LogWarning("RA rate limited on {Endpoint}, attempt {Attempt}/{Max}", endpoint, attempt, maxAttempts);
                if (attempt == maxAttempts)
                {
                    response.EnsureSuccessStatusCode();
                }

                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 60));
                continue;
            }

            response.EnsureSuccessStatusCode();
            var payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
            return payload ?? throw new InvalidOperationException($"Empty response from RA endpoint {endpoint}");
        }

        throw new InvalidOperationException($"Failed to call RA endpoint {endpoint} after retries");
    }

    private string BuildUrl(string endpoint, Dictionary<string, string> query)
    {
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var qs = string.Join("&", query.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
        return $"{baseUrl}/{endpoint}?{qs}";
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("RA__ApiKey is not configured");
        }
    }
}
