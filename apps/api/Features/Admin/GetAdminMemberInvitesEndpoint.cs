using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.Admin;

public static class GetAdminMemberInvitesEndpoint
{
    public static RouteHandlerBuilder MapGetAdminMemberInvites(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/admin/member-invites", async (
            AppDbContext db,
            IOptions<AuthOptions> authOptions,
            CancellationToken ct) =>
        {
            var members = await db.Members
                .Where(m => m.DiscordId != null)
                .OrderBy(m => m.DiscordId)
                .ToListAsync(ct);

            var items = members.Select(m =>
            {
                var discordId = m.DiscordId!;
                var isAdmin = MemberAuthHelper.IsAdmin(m, discordId, authOptions.Value);
                return new AdminMemberInviteDto(
                    discordId,
                    MemberAuthHelper.DisplayLabel(m),
                    !string.IsNullOrWhiteSpace(m.RaUsername),
                    !string.IsNullOrWhiteSpace(m.RaApiKey),
                    isAdmin);
            }).ToList();

            return Results.Ok(items);
        })
        .WithName("GetAdminMemberInvites")
        .WithTags("Admin")
        .WithSummary("Lists Discord-invited members and their onboarding status.")
        .RequireAdmin();
}

public sealed record AdminMemberInviteDto(
    string DiscordId,
    string? DisplayName,
    bool HasRaAccount,
    bool HasApiKey,
    bool IsAdmin);
