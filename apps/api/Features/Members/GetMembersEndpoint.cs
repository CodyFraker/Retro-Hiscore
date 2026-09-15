using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class GetMembersEndpoint
{
    public static RouteHandlerBuilder MapGetMembers(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/members", async (AppDbContext db, CancellationToken ct) =>
        {
            var members = await MembersRosterQuery.LoadAsync(db, ct);
            return Results.Ok(members);
        })
        .WithName("GetMembers")
        .WithTags("Members")
        .WithSummary("Returns tracked members with competitive standings and RetroAchievements roster metrics.")
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
    bool HasApiKey = false,
    int? RaRank = null,
    int? RaTotalRanked = null,
    int? RaTotalPoints = null,
    int? RaTotalSoftcorePoints = null,
    DateTimeOffset? RaMetricsSyncedAt = null,
    int? RaRankDelta = null,
    int? RaPointsDelta = null,
    DateTimeOffset? LastActiveAt = null,
    string? RaStatus = null,
    int? RaPresenceRaGameId = null,
    string? RaPresenceGameTitle = null,
    bool? RaPresenceIsTracked = null,
    DateTimeOffset? RaPresenceSyncedAt = null);
