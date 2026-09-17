using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;

namespace RetroHiscore.Api.Features.Sync;

public static class LeaderboardSyncMemberEligibility
{
    public static async Task<IReadOnlyList<Guid>> GetEligibleMemberIdsAsync(
        AppDbContext db,
        int raGameId,
        DateTimeOffset utcNow,
        LeaderboardSyncPolicy policy,
        CancellationToken cancellationToken = default)
    {
        var windowStart = utcNow - TimeSpan.FromHours(policy.HotActivityWindowHours);

        return await db.MemberRecentGamePlays
            .AsNoTracking()
            .Where(p => p.RaGameId == raGameId && p.LastPlayedAt >= windowStart)
            .Join(
                db.Members.AsNoTracking(),
                p => p.MemberId,
                m => m.Id,
                (p, m) => m)
            .Where(m => m.RaUsername != null && m.RaApiKey != null && m.RaApiKey != "")
            .Select(m => m.Id)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public static async Task<bool> IsMemberEligibleAsync(
        AppDbContext db,
        Guid memberId,
        int raGameId,
        DateTimeOffset utcNow,
        LeaderboardSyncPolicy policy,
        CancellationToken cancellationToken = default)
    {
        var windowStart = utcNow - TimeSpan.FromHours(policy.HotActivityWindowHours);

        return await db.MemberRecentGamePlays
            .AsNoTracking()
            .AnyAsync(
                p => p.MemberId == memberId
                    && p.RaGameId == raGameId
                    && p.LastPlayedAt >= windowStart,
                cancellationToken)
            && await db.Members
                .AsNoTracking()
                .AnyAsync(
                    m => m.Id == memberId
                        && m.RaUsername != null
                        && m.RaApiKey != null
                        && m.RaApiKey != "",
                    cancellationToken);
    }

    public static async Task<IReadOnlyList<int>> GetEligibleTrackedRaGameIdsForMemberAsync(
        AppDbContext db,
        Guid memberId,
        IReadOnlySet<int> trackedRaGameIds,
        DateTimeOffset utcNow,
        LeaderboardSyncPolicy policy,
        CancellationToken cancellationToken = default)
    {
        if (trackedRaGameIds.Count == 0)
        {
            return [];
        }

        var windowStart = utcNow - TimeSpan.FromHours(policy.HotActivityWindowHours);
        var trackedList = trackedRaGameIds.ToList();

        var hasKey = await db.Members
            .AsNoTracking()
            .AnyAsync(
                m => m.Id == memberId
                    && m.RaUsername != null
                    && m.RaApiKey != null
                    && m.RaApiKey != "",
                cancellationToken);

        if (!hasKey)
        {
            return [];
        }

        return await db.MemberRecentGamePlays
            .AsNoTracking()
            .Where(p => p.MemberId == memberId
                && trackedList.Contains(p.RaGameId)
                && p.LastPlayedAt >= windowStart)
            .Select(p => p.RaGameId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
