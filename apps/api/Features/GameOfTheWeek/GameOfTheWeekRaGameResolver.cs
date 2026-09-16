using Microsoft.Extensions.Options;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.GameOfTheWeek;

public sealed record ResolvedRaGameSnapshot(int RaGameId, string Title, string? ConsoleName, string? ImageIcon);

public interface IGameOfTheWeekRaGameResolver
{
    Task<(ResolvedRaGameSnapshot? Snapshot, IResult? Error)> ResolveAsync(int raGameId, CancellationToken ct);
}

public sealed class GameOfTheWeekRaGameResolver(
    IRaApiClient raApiClient,
    IRaApiKeyPool apiKeyPool,
    IOptions<RaOptions> raOptions) : IGameOfTheWeekRaGameResolver
{
    public async Task<(ResolvedRaGameSnapshot? Snapshot, IResult? Error)> ResolveAsync(int raGameId, CancellationToken ct)
    {
        if (raGameId <= 0)
        {
            return (null, Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(raGameId)] = ["raGameId must be a positive integer."]
            }));
        }

        var payload = await apiKeyPool.ExecuteAsync(
            [raOptions.Value.ApiKey],
            (key, token) => raApiClient.GetGameAsync(raGameId, key, token),
            ct);

        if (string.IsNullOrWhiteSpace(payload.Title))
        {
            return (null, Results.NotFound(new { message = $"Game {raGameId} was not found on RetroAchievements." }));
        }

        return (new ResolvedRaGameSnapshot(
            raGameId,
            payload.Title.Trim(),
            string.IsNullOrWhiteSpace(payload.ConsoleName) ? null : payload.ConsoleName.Trim(),
            string.IsNullOrWhiteSpace(payload.ImageIcon) ? null : payload.ImageIcon.Trim()), null);
    }
}
