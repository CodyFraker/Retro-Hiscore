using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Dashboard;

public static class GetDashboardGamesEndpoint
{
    public static RouteHandlerBuilder MapGetDashboardGames(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/dashboard/games", async (
            AppDbContext db,
            IOptions<RaOptions> raOptions,
            int? limit,
            int? offset,
            string? sort,
            string? q,
            CancellationToken ct) =>
        {
            var response = await DashboardGamesQuery.GetPageAsync(
                db,
                raOptions,
                limit,
                offset,
                sort,
                q,
                ct);
            return Results.Ok(response);
        })
        .WithName("GetDashboardGames")
        .WithTags("Dashboard")
        .WithSummary("Returns paginated tracked game cards with search and sort for the dashboard and games index.")
        .RequireApiAuth();
}
