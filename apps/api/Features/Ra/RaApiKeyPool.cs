using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;

namespace RetroHiscore.Api.Features.Ra;

public sealed class RaApiKeyPool(
    AppDbContext db,
    IOptions<RaOptions> options,
    ILogger<RaApiKeyPool> logger) : IRaApiKeyPool
{
    private readonly RaOptions _options = options.Value;

    public async Task<IReadOnlyList<string>> GetMemberKeysAsync(CancellationToken cancellationToken = default)
    {
        return await db.Members
            .Where(m => m.RaApiKey != null && m.RaApiKey != "")
            .Select(m => m.RaApiKey!)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<T> ExecuteAsync<T>(
        IEnumerable<string> preferredKeys,
        Func<string, CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        var memberKeys = await GetMemberKeysAsync(cancellationToken);
        var keys = BuildKeyOrder(preferredKeys, _options.ApiKey, memberKeys);

        if (keys.Count == 0)
        {
            throw new InvalidOperationException("No RA API keys are configured");
        }

        Exception? lastException = null;

        foreach (var key in keys)
        {
            try
            {
                return await operation(key, cancellationToken);
            }
            catch (RaApiRateLimitedException ex)
            {
                lastException = ex;
                logger.LogWarning("RA API key rate limited, trying next key in pool");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                lastException = ex;
                logger.LogWarning("RA API key rate limited, trying next key in pool");
            }
        }

        throw lastException ?? new InvalidOperationException("All RA API keys in the pool failed");
    }

    public static List<string> BuildKeyOrder(
        IEnumerable<string> preferredKeys,
        string envApiKey,
        IReadOnlyList<string> memberKeys)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var ordered = new List<string>();

        void AddKey(string? key)
        {
            if (string.IsNullOrWhiteSpace(key) || !seen.Add(key))
            {
                return;
            }

            ordered.Add(key);
        }

        foreach (var key in preferredKeys)
        {
            AddKey(key);
        }

        AddKey(envApiKey);

        foreach (var key in memberKeys)
        {
            AddKey(key);
        }

        return ordered;
    }
}
