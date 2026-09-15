using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Sync;

internal static class ManualSyncCooldown
{
    public static bool IsActive(
        AppDbContext db,
        SyncKind kind,
        int cooldownSeconds,
        out DateTimeOffset? availableAt)
        => IsActive(db, kind, cooldownSeconds, memberId: null, out availableAt);

    public static bool IsActive(
        AppDbContext db,
        SyncKind kind,
        int cooldownSeconds,
        Guid? memberId,
        out DateTimeOffset? availableAt)
    {
        availableAt = null;
        var query = db.SyncRuns.Where(r => r.Kind == kind && r.Trigger == SyncTrigger.Manual);
        query = memberId.HasValue
            ? query.Where(r => r.MemberId == memberId)
            : query.Where(r => r.MemberId == null);

        var lastManual = query
            .OrderByDescending(r => r.StartedAt)
            .FirstOrDefault();

        if (lastManual is null)
        {
            return false;
        }

        var cooldownEnds = lastManual.StartedAt.AddSeconds(cooldownSeconds);
        if (cooldownEnds <= DateTimeOffset.UtcNow)
        {
            return false;
        }

        availableAt = cooldownEnds;
        return true;
    }
}
