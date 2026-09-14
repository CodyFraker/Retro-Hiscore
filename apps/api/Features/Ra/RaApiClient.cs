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

    public async Task<RaGameDto> GetGameAsync(int gameId, string? apiKey = null, CancellationToken cancellationToken = default)
    {
        var key = ResolveApiKey(apiKey);
        var query = new Dictionary<string, string>
        {
            ["y"] = key,
            ["i"] = gameId.ToString()
        };
        return await GetAsync<RaGameDto>("API_GetGame.php", query, cancellationToken);
    }

    public async Task<IReadOnlyList<RaConsoleIdDto>> GetConsoleIdsAsync(string? apiKey = null, CancellationToken cancellationToken = default)
    {
        var key = ResolveApiKey(apiKey);
        var query = new Dictionary<string, string>
        {
            ["y"] = key,
            ["g"] = "1"
        };
        return await GetAsync<List<RaConsoleIdDto>>("API_GetConsoleIDs.php", query, cancellationToken);
    }

    public Task<IReadOnlyList<RaGameLeaderboardDto>> GetGameLeaderboardsAsync(
        int gameId,
        string? apiKey = null,
        CancellationToken cancellationToken = default)
        => GetAllPagesAsync<RaGameLeaderboardDto>(
            "API_GetGameLeaderboards.php",
            new Dictionary<string, string> { ["i"] = gameId.ToString() },
            apiKey,
            cancellationToken);

    public async Task<IReadOnlyList<RaUserGameLeaderboardDto>> GetUserGameLeaderboardsAsync(
        int gameId,
        string usernameOrUlid,
        string? apiKey = null,
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
                apiKey,
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

    public async Task<int?> GetLeaderboardEntryCountAsync(
        long leaderboardId,
        string? apiKey = null,
        CancellationToken cancellationToken = default)
    {
        var key = ResolveApiKey(apiKey);
        var query = new Dictionary<string, string>
        {
            ["y"] = key,
            ["i"] = leaderboardId.ToString(),
            ["c"] = "1",
            ["o"] = "0"
        };

        var page = await GetAsync<RaPagedResponse<RaLeaderboardEntryDto>>(
            "API_GetLeaderboardEntries.php",
            query,
            cancellationToken);
        return page.Total;
    }

    public async Task<RaUserSummaryDto?> GetUserSummaryAsync(
        string username,
        string apiKey,
        int recentGamesCount = 3,
        int recentAchievementsCount = 8,
        CancellationToken cancellationToken = default)
    {
        var key = ResolveApiKey(apiKey);
        var query = new Dictionary<string, string>
        {
            ["y"] = key,
            ["u"] = username,
            ["g"] = recentGamesCount.ToString(),
            ["a"] = recentAchievementsCount.ToString()
        };

        try
        {
            return await GetAsync<RaUserSummaryDto>("API_GetUserSummary.php", query, cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<RaUserRecentlyPlayedGameDto>> GetUserRecentlyPlayedGamesAsync(
        string usernameOrUlid,
        int count = 15,
        int offset = 0,
        string? apiKey = null,
        CancellationToken cancellationToken = default)
    {
        var key = ResolveApiKey(apiKey);
        var query = new Dictionary<string, string>
        {
            ["y"] = key,
            ["u"] = usernameOrUlid,
            ["c"] = count.ToString(),
            ["o"] = offset.ToString()
        };

        try
        {
            var results = await GetAsync<List<RaUserRecentlyPlayedGameDto>>(
                "API_GetUserRecentlyPlayedGames.php",
                query,
                cancellationToken);
            return results ?? [];
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return [];
        }
    }

    public async Task<RaGameInfoAndUserProgressDto?> GetGameInfoAndUserProgressAsync(
        int gameId,
        string usernameOrUlid,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        var key = ResolveApiKey(apiKey);
        var query = new Dictionary<string, string>
        {
            ["y"] = key,
            ["g"] = gameId.ToString(),
            ["u"] = usernameOrUlid
        };

        try
        {
            return await GetAsync<RaGameInfoAndUserProgressDto>(
                "API_GetGameInfoAndUserProgress.php",
                query,
                cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    private async Task<IReadOnlyList<T>> GetAllPagesAsync<T>(
        string endpoint,
        Dictionary<string, string> query,
        string? apiKey,
        CancellationToken cancellationToken)
    {
        var key = ResolveApiKey(apiKey);
        var results = new List<T>();
        var offset = 0;
        const int pageSize = 500;

        while (true)
        {
            var pageQuery = new Dictionary<string, string>(query)
            {
                ["y"] = key,
                ["c"] = pageSize.ToString(),
                ["o"] = offset.ToString()
            };

            var page = await GetAsync<RaPagedResponse<T>>(endpoint, pageQuery, cancellationToken);
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

    private async Task<T> GetAsync<T>(string endpoint, Dictionary<string, string> query, CancellationToken cancellationToken)
    {
        var url = BuildUrl(endpoint, query);
        using var response = await httpClient.GetAsync(url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new RaApiRateLimitedException($"RA rate limited on {endpoint}");
        }

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        return payload ?? throw new InvalidOperationException($"Empty response from RA endpoint {endpoint}");
    }

    private string ResolveApiKey(string? apiKey)
    {
        var key = string.IsNullOrWhiteSpace(apiKey) ? _options.ApiKey : apiKey;
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("RA API key is not configured");
        }

        return key;
    }

    private string BuildUrl(string endpoint, Dictionary<string, string> query)
    {
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var qs = string.Join("&", query.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
        return $"{baseUrl}/{endpoint}?{qs}";
    }
}
