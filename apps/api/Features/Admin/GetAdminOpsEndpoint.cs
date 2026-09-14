using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin;

public static class GetAdminOpsEndpoint
{
    public static RouteHandlerBuilder MapGetAdminOps(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/admin/ops", async (AdminOpsBuilder builder, CancellationToken ct) =>
        {
            var ops = await builder.BuildAsync(ct);
            return Results.Ok(ops);
        })
        .WithName("GetAdminOps")
        .WithTags("Admin")
        .WithSummary("Returns sync health, run history, member coverage, and scheduler context for administrators.")
        .RequireAdmin();
}
