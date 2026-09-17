using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.GameOfTheWeek;

namespace RetroHiscore.Api.Features.Games;

public interface IGameTrackQueueRequestService
{
    Task<(GameTrackRequestSubmitResponse? Response, IResult? ErrorResult, int StatusCode)> SubmitAsync(
        Guid memberId,
        int raGameId,
        CancellationToken ct);

    Task<GameTrackRequestQuotaDto> GetQuotaAsync(Guid memberId, CancellationToken ct);
}

public sealed class GameTrackQueueRequestService(
    AppDbContext db,
    IGameOfTheWeekRaGameResolver raGameResolver) : IGameTrackQueueRequestService
{
    public async Task<GameTrackRequestQuotaDto> GetQuotaAsync(Guid memberId, CancellationToken ct)
    {
        var windowStart = DateTimeOffset.UtcNow - GameTrackRequestConstants.RollingWindow;
        var recent = await db.GameTrackQueueRequests
            .AsNoTracking()
            .Where(r => r.MemberId == memberId && r.CreatedAt >= windowStart)
            .OrderBy(r => r.CreatedAt)
            .Select(r => r.CreatedAt)
            .ToListAsync(ct);

        return BuildQuota(recent);
    }

    public async Task<(GameTrackRequestSubmitResponse? Response, IResult? ErrorResult, int StatusCode)> SubmitAsync(
        Guid memberId,
        int raGameId,
        CancellationToken ct)
    {
        var tracked = await db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.RaGameId == raGameId, ct);
        if (tracked is not null)
        {
            var body = new GameTrackRequestSubmitResponse(
                "alreadyTracked",
                raGameId,
                tracked.Title,
                $"Game {raGameId} is already tracked.");
            return (body, Results.Conflict(body), StatusCodes.Status409Conflict);
        }

        var (snapshot, resolveError) = await raGameResolver.ResolveAsync(raGameId, ct);
        if (resolveError is not null)
        {
            return (null, resolveError, StatusCodes.Status404NotFound);
        }

        var title = snapshot!.Title;
        var consoleName = snapshot.ConsoleName;
        var now = DateTimeOffset.UtcNow;

        var pending = await db.GameTrackQueues
            .FirstOrDefaultAsync(q => q.RaGameId == raGameId && q.Status == GameTrackQueueStatus.Pending, ct);

        if (pending is not null)
        {
            var alreadyRequested = await db.GameTrackQueueRequests.AnyAsync(
                r => r.MemberId == memberId && r.GameTrackQueueId == pending.Id,
                ct);

            if (alreadyRequested)
            {
                var body = new GameTrackRequestSubmitResponse("alreadyPending", raGameId, pending.Title);
                return (body, Results.Ok(body), StatusCodes.Status200OK);
            }

            var quotaError = await EnsureQuotaAvailableAsync(memberId, raGameId, ct);
            if (quotaError is not null)
            {
                return quotaError.Value;
            }

            db.GameTrackQueueRequests.Add(new GameTrackQueueRequest
            {
                MemberId = memberId,
                RaGameId = raGameId,
                GameTrackQueueId = pending.Id,
                CreatedAt = now
            });
            await db.SaveChangesAsync(ct);
            pending.RequestCount = await db.GameTrackQueueRequests
                .Where(r => r.GameTrackQueueId == pending.Id)
                .Select(r => r.MemberId)
                .Distinct()
                .CountAsync(ct);
            await db.SaveChangesAsync(ct);

            var okBody = new GameTrackRequestSubmitResponse("alreadyPending", raGameId, pending.Title);
            return (okBody, Results.Ok(okBody), StatusCodes.Status200OK);
        }

        var quotaErrorNew = await EnsureQuotaAvailableAsync(memberId, raGameId, ct);
        if (quotaErrorNew is not null)
        {
            return quotaErrorNew.Value;
        }

        var rejected = await db.GameTrackQueues
            .FirstOrDefaultAsync(q => q.RaGameId == raGameId && q.Status == GameTrackQueueStatus.Rejected, ct);

        GameTrackQueue queueRow;
        if (rejected is not null)
        {
            rejected.Status = GameTrackQueueStatus.Pending;
            rejected.Title = title;
            rejected.ConsoleName = consoleName;
            rejected.EnqueuedAt = now;
            rejected.ResolvedAt = null;
            rejected.ResolvedByMemberId = null;
            rejected.FailureMessage = null;
            rejected.Source = GameTrackQueueSource.MemberRequest;
            rejected.RequestedByMemberId = memberId;
            rejected.RequestCount = 1;
            queueRow = rejected;
        }
        else
        {
            queueRow = new GameTrackQueue
            {
                RaGameId = raGameId,
                Status = GameTrackQueueStatus.Pending,
                Title = title,
                ConsoleName = consoleName,
                EnqueuedAt = now,
                Source = GameTrackQueueSource.MemberRequest,
                RequestedByMemberId = memberId,
                RequestCount = 1
            };
            db.GameTrackQueues.Add(queueRow);
        }

        await db.SaveChangesAsync(ct);

        db.GameTrackQueueRequests.Add(new GameTrackQueueRequest
        {
            MemberId = memberId,
            RaGameId = raGameId,
            GameTrackQueueId = queueRow.Id,
            CreatedAt = now
        });
        await db.SaveChangesAsync(ct);

        var response = new GameTrackRequestSubmitResponse("created", raGameId, title);
        return (response, Results.Created($"/api/games/track-requests/{raGameId}", response), StatusCodes.Status201Created);
    }

    private async Task<(GameTrackRequestSubmitResponse? Response, IResult? ErrorResult, int StatusCode)?> EnsureQuotaAvailableAsync(
        Guid memberId,
        int raGameId,
        CancellationToken ct)
    {
        var quota = await GetQuotaAsync(memberId, ct);
        if (quota.Remaining <= 0)
        {
            var body = new GameTrackRequestSubmitResponse(
                "quotaExceeded",
                raGameId,
                string.Empty,
                "You have reached the limit of 5 track requests in 24 hours.",
                quota.NextSlotAt);
            return (body, Results.Json(body, statusCode: StatusCodes.Status429TooManyRequests), StatusCodes.Status429TooManyRequests);
        }

        return null;
    }

    private static GameTrackRequestQuotaDto BuildQuota(IReadOnlyList<DateTimeOffset> recentCreatedAt)
    {
        var limit = GameTrackRequestConstants.MaxPerRollingWindow;
        var used = recentCreatedAt.Count;
        var remaining = Math.Max(0, limit - used);
        DateTimeOffset? nextSlotAt = null;
        if (remaining == 0 && recentCreatedAt.Count > 0)
        {
            nextSlotAt = recentCreatedAt[0] + GameTrackRequestConstants.RollingWindow;
        }

        return new GameTrackRequestQuotaDto(limit, used, remaining, nextSlotAt);
    }
}
