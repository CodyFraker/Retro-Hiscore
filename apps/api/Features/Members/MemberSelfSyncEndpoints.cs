using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class GetMemberSelfSyncStatusEndpoint
{
    public static RouteHandlerBuilder MapGetMemberSelfSyncStatus(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/members/me/sync-status", async (
            ClaimsPrincipal user,
            AppDbContext db,
            IMemberSelfSyncService selfSync,
            CancellationToken ct) =>
        {
            var member = await MemberSelfSyncEndpointHelpers.ResolveMemberAsync(user, db, ct);
            if (member is null)
            {
                return Results.NotFound();
            }

            var status = await selfSync.GetStatusAsync(member, ct);
            return Results.Ok(status);
        })
        .WithName("GetMemberSelfSyncStatus")
        .WithTags("Members")
        .WithSummary("Returns last-synced times and cooldowns for the current member's self-service sync actions.")
        .RequireApiAuth();
}

public static class PostMemberSelfSyncLeaderboardsEndpoint
{
    public static RouteHandlerBuilder MapPostMemberSelfSyncLeaderboards(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/members/me/sync/leaderboards", async (
            ClaimsPrincipal user,
            AppDbContext db,
            IMemberSelfSyncService selfSync,
            CancellationToken ct) =>
        {
            var member = await MemberSelfSyncEndpointHelpers.ResolveMemberAsync(user, db, ct);
            if (member is null)
            {
                return Results.NotFound();
            }

            var denied = MemberSelfSyncEndpointHelpers.RequireApiKey(member);
            if (denied is not null)
            {
                return denied;
            }

            if (selfSync.IsLeaderboardsCooldownActive(member.Id, out var availableAt))
            {
                return Results.Json(
                    new SyncCooldownResponse("Leaderboard sync cooldown is active", availableAt),
                    statusCode: StatusCodes.Status429TooManyRequests);
            }

            var result = await selfSync.QueueLeaderboardsAsync(member, ct);
            PlatformMetrics.RecordMemberSelfSyncRequested("leaderboards");
            return Results.Accepted(
                "/api/members/me/sync-status",
                new MemberSelfSyncLeaderboardsResponse(result.Queued, result.SkippedCooldown));
        })
        .WithName("PostMemberSelfSyncLeaderboards")
        .WithTags("Members")
        .WithSummary("Queues leaderboard score refresh jobs for the current member on every tracked game.")
        .RequireApiAuth();
}

public static class PostMemberSelfSyncProfileEndpoint
{
    public static RouteHandlerBuilder MapPostMemberSelfSyncProfile(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/members/me/sync/profile", async (
            ClaimsPrincipal user,
            AppDbContext db,
            IMemberSelfSyncService selfSync,
            CancellationToken ct) =>
        {
            var member = await MemberSelfSyncEndpointHelpers.ResolveMemberAsync(user, db, ct);
            if (member is null)
            {
                return Results.NotFound();
            }

            var denied = MemberSelfSyncEndpointHelpers.RequireApiKey(member);
            if (denied is not null)
            {
                return denied;
            }

            if (selfSync.IsProfileCooldownActive(member.Id, out var availableAt))
            {
                return Results.Json(
                    new SyncCooldownResponse("Profile sync cooldown is active", availableAt),
                    statusCode: StatusCodes.Status429TooManyRequests);
            }

            var run = await selfSync.SyncProfileAsync(member, ct);
            PlatformMetrics.RecordMemberSelfSyncRequested("profile");
            return Results.Ok(new MemberSelfSyncRunResponse(
                run.Id,
                run.Status.ToString(),
                run.FinishedAt));
        })
        .WithName("PostMemberSelfSyncProfile")
        .WithTags("Members")
        .WithSummary("Syncs RetroAchievements profile data (rank, presence, recently played) for the current member.")
        .RequireApiAuth();
}

public static class PostMemberSelfSyncAchievementsEndpoint
{
    public static RouteHandlerBuilder MapPostMemberSelfSyncAchievements(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/members/me/sync/achievements", async (
            ClaimsPrincipal user,
            AppDbContext db,
            IMemberSelfSyncService selfSync,
            CancellationToken ct) =>
        {
            var member = await MemberSelfSyncEndpointHelpers.ResolveMemberAsync(user, db, ct);
            if (member is null)
            {
                return Results.NotFound();
            }

            var denied = MemberSelfSyncEndpointHelpers.RequireApiKey(member);
            if (denied is not null)
            {
                return denied;
            }

            if (selfSync.IsAchievementsCooldownActive(member.Id, out var availableAt))
            {
                return Results.Json(
                    new SyncCooldownResponse("Achievement sync cooldown is active", availableAt),
                    statusCode: StatusCodes.Status429TooManyRequests);
            }

            var run = await selfSync.SyncAchievementsAsync(member, ct);
            PlatformMetrics.RecordMemberSelfSyncRequested("achievements");
            return Results.Ok(new MemberSelfSyncRunResponse(
                run.Id,
                run.Status.ToString(),
                run.FinishedAt));
        })
        .WithName("PostMemberSelfSyncAchievements")
        .WithTags("Members")
        .WithSummary("Syncs achievement progress on tracked games for the current member.")
        .RequireApiAuth();
}

internal static class MemberSelfSyncEndpointHelpers
{
    internal static async Task<Domain.Member?> ResolveMemberAsync(
        ClaimsPrincipal user,
        AppDbContext db,
        CancellationToken ct)
    {
        var discordId = DiscordUserExtensions.GetDiscordUserId(user);
        if (string.IsNullOrWhiteSpace(discordId))
        {
            return null;
        }

        return await db.Members.FirstOrDefaultAsync(m => m.DiscordId == discordId, ct);
    }

    internal static IResult? RequireApiKey(Domain.Member member)
    {
        if (!string.IsNullOrWhiteSpace(member.RaApiKey) && !string.IsNullOrWhiteSpace(member.RaUsername))
        {
            return null;
        }

        return Results.Json(
            new { message = "Link your RetroAchievements username and API key in Settings before syncing." },
            statusCode: StatusCodes.Status403Forbidden);
    }
}

public sealed record MemberSelfSyncLeaderboardsResponse(int Queued, int SkippedCooldown);

public sealed record MemberSelfSyncRunResponse(
    Guid Id,
    string Status,
    DateTimeOffset? FinishedAt);
