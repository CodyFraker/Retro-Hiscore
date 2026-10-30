using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Dashboard;

public static class GetMonthSummaryEndpoint
{
    public static RouteHandlerBuilder MapGetMonthSummary(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/dashboard/month-summary", async (
            AppDbContext db,
            int? year,
            int? month,
            CancellationToken ct) =>
        {
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

            var response = await MonthSummaryQuery.LoadAsync(db, y, m, ct);
            return Results.Ok(response);
        })
        .WithName("GetMonthSummary")
        .WithTags("Dashboard")
        .WithSummary("Returns a UTC month recap of group games, achievements, and leaderboard activity.")
        .RequireApiAuth();
}
