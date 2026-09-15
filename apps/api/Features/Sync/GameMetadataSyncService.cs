using Hangfire;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.Sync;

public interface IGameMetadataSyncService
{
    Task<SyncRun> SyncAsync(SyncTrigger trigger, CancellationToken cancellationToken = default);
    Task SyncGameAsync(Game game, CancellationToken cancellationToken = default);
    bool IsManualCooldownActive(out DateTimeOffset? availableAt);
}

public sealed class GameMetadataSyncService(
    AppDbContext db,
    IRaApiClient raApiClient,
    IRaApiKeyPool apiKeyPool,
    IConsoleIconSyncService consoleIconSync,
    IGameAchievementDistributionSyncService achievementDistributionSync,
    IOptions<RaOptions> raOptions,
    IOptions<GameMetadataSyncOptions> options,
    ILogger<GameMetadataSyncService> logger) : IGameMetadataSyncService
{
    private readonly RaOptions _raOptions = raOptions.Value;
    private readonly GameMetadataSyncOptions _options = options.Value;

    public bool IsManualCooldownActive(out DateTimeOffset? availableAt)
    {
        availableAt = null;
        var lastManual = db.SyncRuns
            .Where(r => r.Kind == SyncKind.GameMetadata && r.Trigger == SyncTrigger.Manual)
            .OrderByDescending(r => r.StartedAt)
            .FirstOrDefault();

        if (lastManual is null)
        {
            return false;
        }

        var cooldownEnds = lastManual.StartedAt.AddSeconds(_options.ManualCooldownSeconds);
        if (cooldownEnds <= DateTimeOffset.UtcNow)
        {
            return false;
        }

        availableAt = cooldownEnds;
        return true;
    }

    public async Task<SyncRun> SyncAsync(SyncTrigger trigger, CancellationToken cancellationToken = default)
    {
        var run = new SyncRun
        {
            Kind = SyncKind.GameMetadata,
            Trigger = trigger,
            Status = SyncRunStatus.Running,
            StartedAt = DateTimeOffset.UtcNow
        };
        db.SyncRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        var errors = new List<string>();
        var syncedAt = DateTimeOffset.UtcNow;

        try
        {
            var games = await db.Games.ToListAsync(cancellationToken);
            foreach (var game in games)
            {
                try
                {
                    await SyncGameMetadataAsync(game, syncedAt, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to sync metadata for game {GameId}", game.RaGameId);
                    errors.Add($"Game {game.RaGameId}: {ex.Message}");
                }
            }

            run.Status = errors.Count == 0
                ? SyncRunStatus.Succeeded
                : SyncRunStatus.PartialSuccess;
            run.Error = errors.Count == 0 ? null : string.Join("; ", errors);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Game metadata sync run failed");
            run.Status = SyncRunStatus.Failed;
            run.Error = ex.Message;
        }
        finally
        {
            run.FinishedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        try
        {
            await consoleIconSync.SyncAsync(trigger, force: false, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Console icon sync after game metadata sync failed");
        }

        return run;
    }

    public Task SyncGameAsync(Game game, CancellationToken cancellationToken = default)
        => SyncGameMetadataAsync(game, DateTimeOffset.UtcNow, cancellationToken);

    public static void ApplyMetadata(Game game, RaGameDto payload, DateTimeOffset syncedAt)
    {
        if (!string.IsNullOrWhiteSpace(payload.Title))
        {
            game.Title = payload.Title;
        }

        game.ConsoleId = payload.ConsoleId;
        game.ConsoleName = NullIfWhiteSpace(payload.ConsoleName);
        game.ImageIcon = NullIfWhiteSpace(payload.ImageIcon);
        game.ImageTitle = NullIfWhiteSpace(payload.ImageTitle);
        game.ImageIngame = NullIfWhiteSpace(payload.ImageIngame);
        game.ImageBoxArt = NullIfWhiteSpace(payload.ImageBoxArt);
        game.Publisher = NullIfWhiteSpace(payload.Publisher);
        game.Developer = NullIfWhiteSpace(payload.Developer);
        game.Genre = NullIfWhiteSpace(payload.Genre);
        game.ReleasedAt = ParseReleased(payload.Released);
        game.MetadataSyncedAt = syncedAt;
    }

    private async Task SyncGameMetadataAsync(Game game, DateTimeOffset syncedAt, CancellationToken cancellationToken)
    {
        var payload = await apiKeyPool.ExecuteAsync(
            [_raOptions.ApiKey],
            (key, ct) => raApiClient.GetGameAsync(game.RaGameId, key, ct),
            cancellationToken);
        ApplyMetadata(game, payload, syncedAt);
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            await achievementDistributionSync.SyncGameAsync(game, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Achievement distribution sync failed for game {RaGameId}", game.RaGameId);
        }
    }

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DateTimeOffset? ParseReleased(string? released)
    {
        if (string.IsNullOrWhiteSpace(released))
        {
            return null;
        }

        if (!DateTimeOffset.TryParse(
                released,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed))
        {
            return null;
        }

        return parsed.ToUniversalTime();
    }
}

public sealed class GameMetadataSyncJob(IGameMetadataSyncService syncService)
{
    [DisableConcurrentExecution(timeoutInSeconds: 60 * 30)]
    [AutomaticRetry(Attempts = 0)]
    public Task RunScheduledAsync(CancellationToken cancellationToken = default)
        => syncService.SyncAsync(SyncTrigger.Scheduled, cancellationToken);

    [DisableConcurrentExecution(timeoutInSeconds: 60 * 30)]
    [AutomaticRetry(Attempts = 0)]
    public Task RunManualAsync(CancellationToken cancellationToken = default)
        => syncService.SyncAsync(SyncTrigger.Manual, cancellationToken);
}
