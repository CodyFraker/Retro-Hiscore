using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class GetMembersEndpoint
{
    public static RouteHandlerBuilder MapGetMembers(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/members", async (AppDbContext db, CancellationToken ct) =>
        {
            var members = await db.Members
                .Where(m => m.RaUsername != null)
                .Select(m => new
                {
                    m.Id,
                    m.RaUsername,
                    m.RaUlid,
                    DisplayName = m.DisplayName ?? m.RaUsername!,
                    m.AvatarUrl,
                    BoardsWithScore = m.Entries.Count,
                    FriendRankOnes = m.Entries.Count(e => e.FriendRank == 1)
                })
                .OrderByDescending(m => m.FriendRankOnes)
                .ThenBy(m => m.RaUsername)
                .Select(m => new MemberDto(
                    m.Id,
                    m.RaUsername,
                    m.RaUlid,
                    m.DisplayName,
                    m.AvatarUrl,
                    m.BoardsWithScore,
                    m.FriendRankOnes))
                .ToListAsync(ct);

            return Results.Ok(members);
        })
        .WithName("GetMembers")
        .WithTags("Members")
        .WithSummary("Returns tracked members with competitive standings.")
        .RequireApiAuth();
}

public sealed record MemberDto(
    Guid Id,
    string? RaUsername,
    string? RaUlid,
    string DisplayName,
    string? AvatarUrl,
    int BoardsWithScore,
    int FriendRankOnes,
    bool HasApiKey = false);
