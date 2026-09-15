using System.Text.Json;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;

namespace RetroHiscore.Api.Features.Sync;

public interface IGameAchievementDistributionSyncService
{
    Task SyncGameAsync(Game game, CancellationToken cancellationToken = default);
}

public sealed class GameAchievementDistributionSyncService(
    AppDbContext db,
    IRaApiClient raApiClient,
    IRaApiKeyPool apiKeyPool,
    IOptions<RaOptions> raOptions,
    ILogger<GameAchievementDistributionSyncService> logger) : IGameAchievementDistributionSyncService
{
    private readonly RaOptions _raOptions = raOptions.Value;

    public async Task SyncGameAsync(Game game, CancellationToken cancellationToken = default)
    {
        try
        {
            var softcore = await apiKeyPool.ExecuteAsync(
                [_raOptions.ApiKey],
                (key, ct) => raApiClient.GetAchievementDistributionAsync(game.RaGameId, hardcore: false, key, ct),
                cancellationToken);

            var hardcore = await apiKeyPool.ExecuteAsync(
                [_raOptions.ApiKey],
                (key, ct) => raApiClient.GetAchievementDistributionAsync(game.RaGameId, hardcore: true, key, ct),
                cancellationToken);

            game.AchievementDistributionSoftcoreJson = JsonSerializer.Serialize(softcore);
            game.AchievementDistributionHardcoreJson = JsonSerializer.Serialize(hardcore);
            game.AchievementDistributionSyncedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to sync achievement distribution for game {RaGameId}", game.RaGameId);
            throw;
        }
    }
}
