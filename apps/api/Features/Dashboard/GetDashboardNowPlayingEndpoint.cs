using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Dashboard;

public static class GetDashboardNowPlayingEndpoint
{
    public static RouteHandlerBuilder MapGetDashboardNowPlaying(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/dashboard/now-playing", async (
            AppDbContext db,
            IOptions<RaOptions> raOptions,
            CancellationToken ct) =>
        {
            var response = await NowPlayingQuery.LoadAsync(db, raOptions, ct);
            return Results.Ok(response);
        })
        .WithName("GetDashboardNowPlaying")
        .WithTags("Dashboard")
        .WithSummary("Returns group members currently playing a game according to cached RetroAchievements presence.")
        .RequireApiAuth();
}
