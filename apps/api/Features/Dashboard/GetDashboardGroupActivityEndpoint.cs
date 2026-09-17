using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Dashboard;

public static class GetDashboardGroupActivityEndpoint
{
    public static RouteHandlerBuilder MapGetDashboardGroupActivity(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/dashboard/group-activity", async (
            AppDbContext db,
            IOptions<RaOptions> raOptions,
            int? limit,
            CancellationToken ct) =>
        {
            var take = limit ?? 25;
            var response = await GroupActivityBuilder.BuildAsync(
                db,
                raOptions.Value.MediaBaseUrl,
                take,
                ct);
            return Results.Ok(response);
        })
        .WithName("GetDashboardGroupActivity")
        .WithTags("Dashboard")
        .WithSummary("Returns unified group activity since the previous leaderboard snapshot sync.")
        .RequireApiAuth();
}
