using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Notifications;

public interface IDiscordWebhookStore
{
    Task<IReadOnlyList<AdminDiscordWebhookSummaryDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<AdminDiscordWebhookDetailDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AdminDiscordWebhookDetailDto> CreateAsync(UpsertDiscordWebhookRequest request, CancellationToken cancellationToken = default);
    Task<AdminDiscordWebhookDetailDto> UpdateAsync(Guid id, UpsertDiscordWebhookRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminNotificationDispatchRunDto>> ListDispatchRunsAsync(int limit, CancellationToken cancellationToken = default);
}

public sealed class DiscordWebhookStore(
    AppDbContext db,
    IWebhookUrlProtector urlProtector,
    IDiscordWebhookPayloadValidator payloadValidator) : IDiscordWebhookStore
{
    public async Task<IReadOnlyList<AdminDiscordWebhookSummaryDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var rows = await db.DiscordWebhookConfigs
            .AsNoTracking()
            .Include(w => w.EventSubscriptions)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);

        return rows.Select(ToSummary).ToList();
    }

    public async Task<AdminDiscordWebhookDetailDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await db.DiscordWebhookConfigs
            .AsNoTracking()
            .Include(w => w.EventSubscriptions)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        return row is null ? null : ToDetail(row, includeUrl: false);
    }

    public async Task<AdminDiscordWebhookDetailDto> CreateAsync(
        UpsertDiscordWebhookRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        var now = DateTimeOffset.UtcNow;
        var entity = new DiscordWebhookConfig
        {
            Name = request.Name.Trim(),
            Enabled = request.Enabled,
            WebhookUrlProtected = urlProtector.Protect(request.WebhookUrl.Trim()),
            DigestIntervalMinutes = request.DigestIntervalMinutes,
            PayloadTemplateJson = request.PayloadTemplateJson,
            AllowedRaGameIds = request.AllowedRaGameIds?.Count > 0 ? request.AllowedRaGameIds.ToArray() : null,
            CreatedAt = now,
            UpdatedAt = now
        };

        ApplySubscriptions(entity, ParseEventKinds(request.EventKinds));
        db.DiscordWebhookConfigs.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return ToDetail(entity, includeUrl: false);
    }

    public async Task<AdminDiscordWebhookDetailDto> UpdateAsync(
        Guid id,
        UpsertDiscordWebhookRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        var entity = await db.DiscordWebhookConfigs
            .Include(w => w.EventSubscriptions)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken)
            ?? throw new DiscordWebhookValidationException("Webhook not found.");

        entity.Name = request.Name.Trim();
        entity.Enabled = request.Enabled;
        if (!string.IsNullOrWhiteSpace(request.WebhookUrl))
        {
            entity.WebhookUrlProtected = urlProtector.Protect(request.WebhookUrl.Trim());
        }

        entity.DigestIntervalMinutes = request.DigestIntervalMinutes;
        entity.PayloadTemplateJson = request.PayloadTemplateJson;
        entity.AllowedRaGameIds = request.AllowedRaGameIds?.Count > 0 ? request.AllowedRaGameIds.ToArray() : null;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        db.DiscordWebhookEventSubscriptions.RemoveRange(entity.EventSubscriptions);
        entity.EventSubscriptions.Clear();
        ApplySubscriptions(entity, ParseEventKinds(request.EventKinds));
        await db.SaveChangesAsync(cancellationToken);
        return ToDetail(entity, includeUrl: false);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.DiscordWebhookConfigs.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        db.DiscordWebhookConfigs.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminNotificationDispatchRunDto>> ListDispatchRunsAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        var take = Math.Clamp(limit, 1, 50);
        return await db.NotificationDispatchRuns
            .AsNoTracking()
            .OrderByDescending(r => r.StartedAt)
            .Take(take)
            .Select(r => new AdminNotificationDispatchRunDto(
                r.Id,
                r.Status.ToString(),
                r.StartedAt,
                r.FinishedAt,
                r.Error,
                r.EventsProcessed,
                r.PostsSucceeded,
                r.PostsFailed))
            .ToListAsync(cancellationToken);
    }

    private void ValidateRequest(UpsertDiscordWebhookRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new DiscordWebhookValidationException("Name is required.");
        }

        if (request.DigestIntervalMinutes is < 0 or > 1440)
        {
            throw new DiscordWebhookValidationException("Digest interval must be between 0 and 1440 minutes.");
        }

        var kinds = ParseEventKinds(request.EventKinds);
        var allowed = DiscordTokenCatalog.AllowedCodesForKinds(kinds);
        payloadValidator.ValidateTemplate(request.PayloadTemplateJson, allowed);
    }

    private static void ApplySubscriptions(DiscordWebhookConfig entity, IReadOnlyList<DiscordNotificationEventKind> kinds)
    {
        foreach (var kind in kinds.Distinct())
        {
            entity.EventSubscriptions.Add(new DiscordWebhookEventSubscription
            {
                WebhookConfigId = entity.Id,
                EventKind = kind
            });
        }
    }

    private AdminDiscordWebhookSummaryDto ToSummary(DiscordWebhookConfig row)
    {
        var maskedUrl = DiscordWebhookUrlMasking.Mask(TryUnprotect(row));
        return new AdminDiscordWebhookSummaryDto(
            row.Id,
            row.Name,
            row.Enabled,
            maskedUrl,
            row.DigestIntervalMinutes,
            row.EventSubscriptions.Select(s => s.EventKind.ToString()).OrderBy(x => x).ToList(),
            row.UpdatedAt);
    }

    private AdminDiscordWebhookDetailDto ToDetail(DiscordWebhookConfig row, bool includeUrl)
    {
        var url = includeUrl ? TryUnprotect(row) : DiscordWebhookUrlMasking.Mask(TryUnprotect(row));
        return new AdminDiscordWebhookDetailDto(
            row.Id,
            row.Name,
            row.Enabled,
            url,
            row.DigestIntervalMinutes,
            row.PayloadTemplateJson,
            row.EventSubscriptions.Select(s => s.EventKind.ToString()).Distinct().OrderBy(k => k).ToList(),
            row.AllowedRaGameIds?.ToList(),
            row.UpdatedAt);
    }

    private static IReadOnlyList<DiscordNotificationEventKind> ParseEventKinds(IReadOnlyList<string> kinds)
    {
        if (kinds is null || kinds.Count == 0)
        {
            throw new DiscordWebhookValidationException("At least one event kind is required.");
        }

        var parsed = new List<DiscordNotificationEventKind>();
        foreach (var kind in kinds)
        {
            if (!Enum.TryParse<DiscordNotificationEventKind>(kind, ignoreCase: true, out var value))
            {
                throw new DiscordWebhookValidationException($"Unknown event kind '{kind}'.");
            }

            parsed.Add(value);
        }

        return parsed;
    }

    private string TryUnprotect(DiscordWebhookConfig row)
    {
        try
        {
            return urlProtector.Unprotect(row.WebhookUrlProtected);
        }
        catch
        {
            return "";
        }
    }
}

public sealed class DiscordWebhookValidationException(string message) : Exception(message);

public sealed record AdminDiscordWebhookSummaryDto(
    Guid Id,
    string Name,
    bool Enabled,
    string WebhookUrlMasked,
    int DigestIntervalMinutes,
    IReadOnlyList<string> EventKinds,
    DateTimeOffset UpdatedAt);

public sealed record AdminDiscordWebhookDetailDto(
    Guid Id,
    string Name,
    bool Enabled,
    string WebhookUrlMasked,
    int DigestIntervalMinutes,
    string PayloadTemplateJson,
    IReadOnlyList<string> EventKinds,
    IReadOnlyList<int>? AllowedRaGameIds,
    DateTimeOffset UpdatedAt);

public sealed record UpsertDiscordWebhookRequest(
    string Name,
    bool Enabled,
    string WebhookUrl,
    int DigestIntervalMinutes,
    string PayloadTemplateJson,
    IReadOnlyList<string> EventKinds,
    IReadOnlyList<int>? AllowedRaGameIds);

public sealed record AdminNotificationDispatchRunDto(
    Guid Id,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    string? Error,
    int EventsProcessed,
    int PostsSucceeded,
    int PostsFailed);

public sealed record DiscordTokenCatalogDto(
    IReadOnlyList<DiscordTokenCatalogEntryDto> Events);

public sealed record DiscordTokenCatalogEntryDto(
    string EventKind,
    IReadOnlyList<DiscordTokenCatalogTokenDto> Tokens);

public sealed record DiscordTokenCatalogTokenDto(string Code, string Label);

public sealed record PreviewDiscordWebhookRequest(
    string EventKind,
    string? PayloadTemplateJson);

public sealed record PreviewDiscordWebhookResponse(string RenderedPayloadJson);

public sealed record TestDiscordWebhookRequest(string EventKind);
