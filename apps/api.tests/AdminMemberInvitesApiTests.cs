using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Admin;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class AdminMemberInvitesApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public AdminMemberInvitesApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PostMemberInvite_CreatesPendingMember()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);
        const string inviteId = "222222222222222222";

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/admin/member-invites",
            new PostAdminMemberInviteRequest(inviteId));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var member = await db.Members.SingleAsync(m => m.DiscordId == inviteId);
        member.RaUsername.ShouldBeNull();
    }

    [Fact]
    public async Task PostMemberInvite_ReturnsForbidden_ForNonAdmin()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/admin/member-invites",
            new PostAdminMemberInviteRequest("333333333333333333"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteMemberInvite_RemovesPendingInvite()
    {
        // Arrange
        var adminClient = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);
        const string inviteId = "444444444444444444";
        await adminClient.PostAsJsonAsync(
            "/api/admin/member-invites",
            new PostAdminMemberInviteRequest(inviteId));

        // Act
        var response = await adminClient.DeleteAsync($"/api/admin/member-invites/{inviteId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Members.AnyAsync(m => m.DiscordId == inviteId)).ShouldBeFalse();
    }
}
