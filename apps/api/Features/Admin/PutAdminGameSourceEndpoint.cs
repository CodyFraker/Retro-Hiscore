using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin;

public static class PutAdminGameSourceEndpoint
{
    public static RouteHandlerBuilder MapPutAdminGameSource(this IEndpointRouteBuilder routes)
        => routes.MapPut("/api/admin/games/{raGameId:int}/sources/{sourceId:guid}", async (
            int raGameId,
            Guid sourceId,
            UpsertGameSourceRequest request,
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

            var urlError = GameSourceValidation.ValidateUrl(request.Url);
            if (urlError is not null)
            {
                return Results.ValidationProblem(urlError);
            }

            var typeError = GameSourceValidation.ValidateSourceType(request.SourceType, out var sourceType);
            if (typeError is not null)
            {
                return Results.ValidationProblem(typeError);
            }

            source.SourceType = sourceType;
            source.Url = request.Url.Trim();
            source.Label = GameSourceValidation.NormalizeOptional(request.Label, 128);
            source.SortOrder = request.SortOrder;
            source.Note = GameSourceValidation.NormalizeOptional(request.Note, 512);
            await db.SaveChangesAsync(ct);

            return Results.Ok(GameSourceMapping.ToDto(source));
        })
        .WithName("PutAdminGameSource")
        .WithTags("Admin")
        .WithSummary("Updates a download mirror link for a tracked game.")
        .RequireAdmin();
}
