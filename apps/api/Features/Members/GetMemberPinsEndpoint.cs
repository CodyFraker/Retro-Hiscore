using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class GetMemberPinsEndpoint
{
    public static RouteHandlerBuilder MapGetMemberPins(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/members/me/pins", async (
            ClaimsPrincipal user,
            AppDbContext db,
            IOptions<RaOptions> raOptions,
            CancellationToken ct) =>
        {
            var member = await MemberRequestHelper.ResolveCurrentMemberAsync(user, db, ct);
            if (member is null)
            {
                return Results.NotFound();
            }

            var mediaBaseUrl = raOptions.Value.MediaBaseUrl;
            var pinRows = await db.MemberGamePins
                .AsNoTracking()
                .Include(p => p.Game)
                .Where(p => p.MemberId == member.Id)
                .OrderByDescending(p => p.PinnedAt)
                .ToListAsync(ct);

            var items = pinRows.Select(row =>
            {
                var images = RaMediaUrl.FromGame(row.Game, mediaBaseUrl);
                return new MemberPinDto(
                    row.Game.RaGameId,
                    row.Game.Title,
                    row.Game.ConsoleName,
                    images.ImageBoxArtUrl,
                    images.ImageIconUrl,
                    row.PinnedAt);
            }).ToList();

            return Results.Ok(new MemberPinsResponse(items));
        })
        .WithName("GetMemberPins")
        .WithTags("Members")
        .WithSummary("Returns games pinned by the current member.")
        .RequireApiAuth();
}

public sealed record MemberPinDto(
    int RaGameId,
    string Title,
    string? ConsoleName,
    string? ImageBoxArtUrl,
    string? ImageIconUrl,
    DateTimeOffset PinnedAt);

public sealed record MemberPinsResponse(IReadOnlyList<MemberPinDto> Items);
