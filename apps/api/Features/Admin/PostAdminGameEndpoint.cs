using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin;

public static class PostAdminGameEndpoint
{
    public static RouteHandlerBuilder MapPostAdminGame(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/admin/games", (PostAdminGameRequest request) =>
            Results.Conflict(new
            {
                message = "Games are tracked when they win game-of-the-week voting. Start a poll under Admin → Game of the week."
            }))
        .WithName("PostAdminGame")
        .WithTags("Admin")
        .WithSummary("Direct game tracking is disabled; use game-of-the-week voting instead.")
        .RequireAdmin();
}
