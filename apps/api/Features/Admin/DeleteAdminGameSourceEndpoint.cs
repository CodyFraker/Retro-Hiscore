using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin;

public static class DeleteAdminGameSourceEndpoint
{
    public static RouteHandlerBuilder MapDeleteAdminGameSource(this IEndpointRouteBuilder routes)
        => routes.MapDelete("/api/admin/games/{raGameId:int}/sources/{sourceId:guid}", async (
            int raGameId,
            Guid sourceId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var source = await db.GameSources
                .Include(s => s.Game)
                .FirstOrDefaultAsync(s => s.Id == sourceId && s.Game.RaGameId == raGameId, ct);

            if (source is null)
            {
                return Results.NotFound();
            }

            db.GameSources.Remove(source);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        })
        .WithName("DeleteAdminGameSource")
        .WithTags("Admin")
        .WithSummary("Removes a download mirror link from a tracked game.")
        .RequireAdmin();
}
