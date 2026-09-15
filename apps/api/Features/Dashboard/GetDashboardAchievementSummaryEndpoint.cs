using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Dashboard;

public static class GetDashboardAchievementSummaryEndpoint
{
    public static RouteHandlerBuilder MapGetDashboardAchievementSummary(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/dashboard/achievement-summary", HandleAsync)
        .WithName("GetDashboardAchievementSummary")
        .WithTags("Dashboard")
        .WithSummary("Returns aggregate achievement unlock stats for tracked games.")
        .RequireApiAuth();

    private static async Task<IResult> HandleAsync(AppDbContext db, CancellationToken ct)
    {
        var summary = await DashboardAchievementSummaryQuery.LoadAsync(db, ct);
        return Results.Ok(summary);
    }
}

public sealed record DashboardAchievementTopGameDto(
    int RaGameId,
    string Title,
    int UnlockCount);

public sealed record DashboardAchievementSummaryResponse(
    int UnlocksLast7Days,
    int UnlocksLast30Days,
    int ActiveMembersLast7Days,
    DateTimeOffset? LastUnlockAt,
    DashboardAchievementTopGameDto? TopGameLast7Days,
    DateTimeOffset? AchievementsSyncedAt);
