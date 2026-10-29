using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class GetMemberAlertsEndpoint
{
    public static RouteHandlerBuilder MapGetMemberAlerts(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/members/me/alerts", async (
            ClaimsPrincipal user,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var member = await MemberRequestHelper.ResolveCurrentMemberAsync(user, db, ct);
            if (member is null)
            {
                return Results.NotFound();
            }

            const int take = 50;
            var alerts = await db.MemberAlerts
                .AsNoTracking()
                .Include(a => a.ActorMember)
                .Where(a => a.RecipientMemberId == member.Id)
                .OrderByDescending(a => a.OccurredAt)
                .Take(take)
                .ToListAsync(ct);

            var unreadCount = await db.MemberAlerts
                .CountAsync(a => a.RecipientMemberId == member.Id && a.ReadAt == null, ct);

            var items = alerts.Select(a => new MemberAlertDto(
                a.Id,
                a.Kind.ToString(),
                a.ActorMemberId,
                a.ActorMember is null
                    ? null
                    : MemberAuthHelper.DisplayLabel(a.ActorMember),
                a.ActorMember?.RaUsername,
                a.ActorMember?.AvatarUrl,
                a.RaGameId,
                a.GameTitle,
                a.RaLeaderboardId,
                a.LeaderboardTitle,
                a.AchievementTitle,
                a.AchievementPoints,
                a.Hardcore,
                a.OccurredAt,
                a.ReadAt)).ToList();

            return Results.Ok(new MemberAlertsResponse(items, unreadCount));
        })
        .WithName("GetMemberAlerts")
        .WithTags("Members")
        .WithSummary("Returns the latest personal alerts and unread count for the current member.")
        .RequireApiAuth();
}

public sealed record MemberAlertDto(
    Guid Id,
    string Kind,
    Guid? ActorMemberId,
    string? ActorDisplayName,
    string? ActorRaUsername,
    string? ActorAvatarUrl,
    int? RaGameId,
    string? GameTitle,
    long? RaLeaderboardId,
    string? LeaderboardTitle,
    string? AchievementTitle,
    int? AchievementPoints,
    bool? Hardcore,
    DateTimeOffset OccurredAt,
    DateTimeOffset? ReadAt);

public sealed record MemberAlertsResponse(
    IReadOnlyList<MemberAlertDto> Items,
    int UnreadCount);
