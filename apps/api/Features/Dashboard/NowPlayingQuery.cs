using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Dashboard;

public static class NowPlayingQuery
{
    public static async Task<NowPlayingResponse> LoadAsync(
        AppDbContext db,
        IOptions<RaOptions> raOptions,
        CancellationToken cancellationToken)
    {
        var trackedGameIds = await db.Games.AsNoTracking().Select(g => g.RaGameId).ToHashSetAsync(cancellationToken);

        var members = await db.Members
            .AsNoTracking()
            .Where(m => m.RaUsername != null
                && m.RaPresenceGameTitle != null
                && m.RaStatus != null)
            .ToListAsync(cancellationToken);

        var items = members
            .Where(m => IsOnlineOrPlaying(m.RaStatus) && !string.IsNullOrWhiteSpace(m.RaPresenceGameTitle))
            .Select(m => new NowPlayingItemDto(
                m.Id,
                m.RaUsername!,
                MemberAuthHelper.DisplayLabel(m),
                m.AvatarUrl,
                m.RaPresenceRaGameId ?? 0,
                m.RaPresenceGameTitle!,
                m.RaPresenceRaGameId is int id && trackedGameIds.Contains(id),
                null,
                m.RaPresenceRichPresenceAt))
            .Where(i => i.RaGameId > 0)
            .OrderBy(i => i.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new NowPlayingResponse(items);
    }

    private static bool IsOnlineOrPlaying(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return false;
        }

        var normalized = status.ToLowerInvariant();
        return normalized == "online" || normalized.Contains("playing");
    }
}

public sealed record NowPlayingItemDto(
    Guid MemberId,
    string RaUsername,
    string DisplayName,
    string? AvatarUrl,
    int RaGameId,
    string GameTitle,
    bool IsTracked,
    string? RichPresenceMsg,
    DateTimeOffset? RichPresenceAt);

public sealed record NowPlayingResponse(IReadOnlyList<NowPlayingItemDto> Items);
