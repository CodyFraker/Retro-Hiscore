using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Members;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.GameOfTheWeek;

public static class GetGameOfTheWeekHistoryEndpoint
{
    public static RouteHandlerBuilder MapGetGameOfTheWeekHistory(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/game-of-the-week/history", async (
            ClaimsPrincipal user,
            AppDbContext db,
            int? limit,
            int? offset,
            CancellationToken ct) =>
        {
            var member = await MemberSelfSyncEndpointHelpers.ResolveMemberAsync(user, db, ct);
            if (member is null)
            {
                return Results.NotFound();
            }

            var take = Math.Clamp(limit ?? 10, 1, 50);
            var skip = Math.Max(offset ?? 0, 0);

            var query = db.GameOfTheWeekPolls
                .AsNoTracking()
                .Where(p =>
                    p.ClosedAt != null
                    && p.TrackingStatus != GameOfTheWeekTrackingStatus.Pending);

            var total = await query.CountAsync(ct);
            var polls = await query
                .OrderByDescending(p => p.ClosedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);

            var response = await GameOfTheWeekHistoryMapper.MapPageAsync(db, polls, total, skip, take, ct);
            return Results.Ok(response);
        })
        .WithName("GetGameOfTheWeekHistory")
        .WithTags("GameOfTheWeek")
        .WithSummary("Returns paginated history of completed game-of-the-week polls and winners.")
        .RequireApiAuth();
}
