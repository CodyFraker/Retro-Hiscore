using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin;

public static class PostAdminGameSourceEndpoint
{
    public static RouteHandlerBuilder MapPostAdminGameSource(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/admin/games/{raGameId:int}/sources", async (
            int raGameId,
            UpsertGameSourceRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var game = await db.Games.FirstOrDefaultAsync(g => g.RaGameId == raGameId, ct);
            if (game is null)
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

            var source = new GameSource
            {
                GameId = game.Id,
                SourceType = sourceType,
                Url = request.Url.Trim(),
                Label = GameSourceValidation.NormalizeOptional(request.Label, 128),
                SortOrder = request.SortOrder,
                Note = GameSourceValidation.NormalizeOptional(request.Note, 512)
            };

            db.GameSources.Add(source);
            await db.SaveChangesAsync(ct);

            return Results.Created(
                $"/api/admin/games/{raGameId}/sources/{source.Id}",
                GameSourceMapping.ToDto(source));
        })
        .WithName("PostAdminGameSource")
        .WithTags("Admin")
        .WithSummary("Adds a download mirror link for a tracked game.")
        .RequireAdmin();
}
