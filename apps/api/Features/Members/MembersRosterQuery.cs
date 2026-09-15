using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Members;

internal static class MembersRosterQuery
{
    internal sealed record MemberRow(
        Guid Id,
        string RaUsername,
        string? RaUlid,
        string DisplayName,
        string? AvatarUrl,
        int BoardsWithScore,
        int FriendRankOnes,
        bool HasApiKey,
        string? RaStatus,
        int? RaPresenceRaGameId,
        string? RaPresenceGameTitle,
        DateTimeOffset? RaPresenceRichPresenceAt,
        DateTimeOffset? RaPresenceSyncedAt);

    public static async Task<IReadOnlyList<MemberDto>> LoadAsync(AppDbContext db, CancellationToken ct)
    {
        var queried = await db.Members
            .AsNoTracking()
            .Where(m => m.RaUsername != null)
            .Select(m => new
            {
                m.Id,
                RaUsername = m.RaUsername!,
                m.RaUlid,
                DisplayName = m.DisplayName ?? m.RaUsername!,
                m.AvatarUrl,
                BoardsWithScore = m.Entries.Count,
                FriendRankOnes = m.Entries.Count(e => e.FriendRank == 1),
                HasApiKey = m.RaApiKey != null && m.RaApiKey != "",
                m.RaStatus,
                m.RaPresenceRaGameId,
                m.RaPresenceGameTitle,
                m.RaPresenceRichPresenceAt,
                m.RaPresenceSyncedAt
            })
            .OrderByDescending(m => m.FriendRankOnes)
            .ThenBy(m => m.RaUsername)
            .ToListAsync(ct);

        var rows = queried
            .Select(m => new MemberRow(
                m.Id,
                m.RaUsername,
                m.RaUlid,
                m.DisplayName,
                m.AvatarUrl,
                m.BoardsWithScore,
                m.FriendRankOnes,
                m.HasApiKey,
                m.RaStatus,
                m.RaPresenceRaGameId,
                m.RaPresenceGameTitle,
                m.RaPresenceRichPresenceAt,
                m.RaPresenceSyncedAt))
            .ToList();

        if (rows.Count == 0)
        {
            return [];
        }

        var memberIds = rows.Select(m => m.Id).ToList();
        var trackedGameIds = await db.Games.AsNoTracking().Select(g => g.RaGameId).ToHashSetAsync(ct);

        var snapshots = await db.MemberRaRankSnapshots
            .AsNoTracking()
            .Where(s => memberIds.Contains(s.MemberId))
            .OrderByDescending(s => s.SyncedAt)
            .ToListAsync(ct);

        var latestSnapshot = new Dictionary<Guid, MemberRaRankSnapshot>();
        var previousSnapshot = new Dictionary<Guid, MemberRaRankSnapshot>();
        foreach (var group in snapshots.GroupBy(s => s.MemberId))
        {
            var ordered = group.OrderByDescending(s => s.SyncedAt).ToList();
            latestSnapshot[group.Key] = ordered[0];
            if (ordered.Count > 1)
            {
                previousSnapshot[group.Key] = ordered[1];
            }
        }

        var lastPlayedByMember = await db.MemberRecentGamePlays
            .AsNoTracking()
            .Where(p => memberIds.Contains(p.MemberId))
            .GroupBy(p => p.MemberId)
            .Select(g => new { MemberId = g.Key, At = g.Max(p => p.LastPlayedAt) })
            .ToDictionaryAsync(x => x.MemberId, x => x.At, ct);

        var lastUnlockByMember = await db.MemberRaAchievements
            .AsNoTracking()
            .Where(m => memberIds.Contains(m.MemberId) && m.DateEarned != null)
            .GroupBy(m => m.MemberId)
            .Select(g => new { MemberId = g.Key, At = g.Max(m => m.DateEarned) })
            .ToDictionaryAsync(x => x.MemberId, x => x.At, ct);

        var result = new List<MemberDto>(rows.Count);
        foreach (var row in rows)
        {
            latestSnapshot.TryGetValue(row.Id, out var latest);
            previousSnapshot.TryGetValue(row.Id, out var previous);

            int? rankDelta = null;
            if (latest?.Rank is int latestRank && previous?.Rank is int previousRank)
            {
                rankDelta = previousRank - latestRank;
            }

            int? pointsDelta = null;
            if (latest?.TotalPoints is int latestPoints && previous?.TotalPoints is int previousPoints)
            {
                pointsDelta = latestPoints - previousPoints;
            }

            var lastActiveAt = MaxDate(
                lastPlayedByMember.GetValueOrDefault(row.Id),
                lastUnlockByMember.GetValueOrDefault(row.Id),
                row.RaPresenceRichPresenceAt);

            bool? presenceIsTracked = row.RaPresenceRaGameId is int gameId
                ? trackedGameIds.Contains(gameId)
                : null;

            result.Add(new MemberDto(
                row.Id,
                row.RaUsername,
                row.RaUlid,
                row.DisplayName,
                row.AvatarUrl,
                row.BoardsWithScore,
                row.FriendRankOnes,
                row.HasApiKey,
                latest?.Rank,
                latest?.TotalRanked,
                latest?.TotalPoints,
                latest?.TotalSoftcorePoints,
                latest?.SyncedAt,
                rankDelta,
                pointsDelta,
                lastActiveAt,
                row.RaStatus,
                row.RaPresenceRaGameId,
                row.RaPresenceGameTitle,
                presenceIsTracked,
                row.RaPresenceSyncedAt));
        }

        return result;
    }

    private static DateTimeOffset? MaxDate(DateTimeOffset? a, DateTimeOffset? b, DateTimeOffset? c)
    {
        DateTimeOffset? max = null;
        if (a is not null && (max is null || a > max))
        {
            max = a;
        }

        if (b is not null && (max is null || b > max))
        {
            max = b;
        }

        if (c is not null && (max is null || c > max))
        {
            max = c;
        }

        return max;
    }
}
