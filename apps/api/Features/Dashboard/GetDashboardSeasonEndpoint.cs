using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Dashboard;

public static class GetDashboardSeasonEndpoint
{
    public static RouteHandlerBuilder MapGetDashboardSeason(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/dashboard/seasons", async (
            AppDbContext db,
            ISeasonBoardTakeBackfillService backfill,
            int? year,
            int? month,
            CancellationToken ct) =>
        {
            await backfill.EnsureBackfilledAsync(ct);

            var now = DateTimeOffset.UtcNow;
            var y = year ?? now.Year;
            var m = month ?? now.Month;

            if (m is < 1 or > 12 || y is < 2000 or > 2100)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["month"] = ["month must be between 1 and 12."],
                    ["year"] = ["year must be between 2000 and 2100."]
                });
            }

            var response = await SeasonStandingsQuery.LoadAsync(db, y, m, ct);
            return Results.Ok(response);
        })
        .WithName("GetDashboardSeason")
        .WithTags("Dashboard")
        .WithSummary("Returns monthly season standings based on friend-rank-1 board takes in the requested UTC month.")
        .RequireApiAuth();
}
