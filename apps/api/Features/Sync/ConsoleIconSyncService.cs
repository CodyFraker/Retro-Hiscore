using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Infrastructure;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.Sync;

public interface IConsoleIconSyncService
{
    Task<SyncRun> SyncAsync(SyncTrigger trigger, bool force = false, CancellationToken cancellationToken = default);
    Task EnsureConsoleIconAsync(int consoleId, CancellationToken cancellationToken = default);
    bool IsManualCooldownActive(out DateTimeOffset? availableAt);
}

public sealed class ConsoleIconSyncService(
    AppDbContext db,
    IRaApiClient raApiClient,
    IRaApiKeyPool apiKeyPool,
    IConsoleIconDownloader iconDownloader,
    IOptions<RaOptions> raOptions,
    IOptions<ConsoleIconSyncOptions> options,
    ILogger<ConsoleIconSyncService> logger) : IConsoleIconSyncService
{
    private readonly RaOptions _raOptions = raOptions.Value;
    private readonly ConsoleIconSyncOptions _options = options.Value;

    public bool IsManualCooldownActive(out DateTimeOffset? availableAt)
    {
        availableAt = null;
        var lastManual = db.SyncRuns
            .Where(r => r.Kind == SyncKind.ConsoleIcons && r.Trigger == SyncTrigger.Manual)
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

    public async Task EnsureConsoleIconAsync(int consoleId, CancellationToken cancellationToken = default)
    {
        var existing = await db.Consoles.FindAsync([consoleId], cancellationToken);
        if (!_options.ForceRefresh
            && existing?.IconData is { Length: > 0 })
        {
            return;
        }

        await SyncAsync(SyncTrigger.Scheduled, force: false, consoleIds: [consoleId], cancellationToken);
    }

    public Task<SyncRun> SyncAsync(SyncTrigger trigger, bool force = false, CancellationToken cancellationToken = default)
        => SyncAsync(trigger, force, consoleIds: null, cancellationToken);

    private async Task<SyncRun> SyncAsync(
        SyncTrigger trigger,
        bool force,
        IReadOnlyCollection<int>? consoleIds,
        CancellationToken cancellationToken)
    {
        var run = new SyncRun
        {
            Kind = SyncKind.ConsoleIcons,
            Trigger = trigger,
            Status = SyncRunStatus.Running,
            StartedAt = DateTimeOffset.UtcNow
        };
        db.SyncRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        var errors = new List<string>();
        var effectiveForce = force || _options.ForceRefresh;

        try
        {
            var neededIds = consoleIds is null
                ? await db.Games
                    .Where(g => g.ConsoleId != null)
                    .Select(g => g.ConsoleId!.Value)
                    .Distinct()
                    .ToListAsync(cancellationToken)
                : consoleIds.ToList();

            if (neededIds.Count == 0)
            {
                run.Status = SyncRunStatus.Succeeded;
                return run;
            }

            var raConsoles = await apiKeyPool.ExecuteAsync(
                [_raOptions.ApiKey],
                (key, ct) => raApiClient.GetConsoleIdsAsync(key, ct),
                cancellationToken);
            var byId = raConsoles.ToDictionary(c => c.Id);

            foreach (var consoleId in neededIds)
            {
                try
                {
                    if (!byId.TryGetValue(consoleId, out var raConsole))
                    {
                        errors.Add($"Console {consoleId}: not found in RA console list");
                        continue;
                    }

                    await SyncOneAsync(raConsole, effectiveForce, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to sync console icon {ConsoleId}", consoleId);
                    errors.Add($"Console {consoleId}: {ex.Message}");
                }
            }

            run.Status = errors.Count == 0
                ? SyncRunStatus.Succeeded
                : SyncRunStatus.PartialSuccess;
            run.Error = errors.Count == 0 ? null : string.Join("; ", errors);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Console icon sync run failed");
            run.Status = SyncRunStatus.Failed;
            run.Error = ex.Message;
        }
        finally
        {
            run.FinishedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            SyncRunMetrics.Record(run);
        }

        return run;
    }

    private async Task SyncOneAsync(RaConsoleIdDto raConsole, bool force, CancellationToken cancellationToken)
    {
        var existing = await db.Consoles.FindAsync([raConsole.Id], cancellationToken);

        if (!force && existing?.IconData is { Length: > 0 })
        {
            if (!string.Equals(existing.Name, raConsole.Name, StringComparison.Ordinal))
            {
                existing.Name = raConsole.Name;
                await db.SaveChangesAsync(cancellationToken);
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(raConsole.IconUrl))
        {
            throw new InvalidOperationException("IconURL is missing");
        }

        var download = await iconDownloader.DownloadAsync(raConsole.IconUrl, cancellationToken);
        var syncedAt = DateTimeOffset.UtcNow;
        if (existing is null)
        {
            db.Consoles.Add(new RaConsole
            {
                RaConsoleId = raConsole.Id,
                Name = raConsole.Name,
                IconData = download.Data,
                IconContentType = download.ContentType,
                IconSyncedAt = syncedAt
            });
        }
        else
        {
            existing.Name = raConsole.Name;
            existing.IconData = download.Data;
            existing.IconContentType = download.ContentType;
            existing.IconSyncedAt = syncedAt;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public static string? ToDataUrl(byte[]? data, string? contentType)
    {
        if (data is null || data.Length == 0)
        {
            return null;
        }

        var mime = string.IsNullOrWhiteSpace(contentType) ? "image/png" : contentType.Trim();
        return $"data:{mime};base64,{Convert.ToBase64String(data)}";
    }
}
