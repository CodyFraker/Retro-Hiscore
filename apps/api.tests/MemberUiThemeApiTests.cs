using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Members;
using RetroHiscore.Api.Infrastructure;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class MemberUiThemeApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public MemberUiThemeApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PutUiTheme_Persists_ForLinkedMember()
    {
        // Arrange
        await LinkMemberAsync(AuthTestHelper.AllowedDiscordUserId, "ShrimpPoboy");
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/members/me/ui-theme",
            new PutMemberUiThemeRequest(UiThemes.FrutigerAero));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
        member.UiTheme.ShouldBe(UiThemes.FrutigerAero);
    }

    [Fact]
    public async Task GetCurrentMember_ReturnsSteamUiTheme_ByDefault()
    {
        // Arrange
        await LinkMemberAsync(AuthTestHelper.AllowedDiscordUserId, "ShrimpPoboy");
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var member = await client.GetFromJsonAsync<CurrentMemberDto>("/api/members/me");

        // Assert
        member.ShouldNotBeNull();
        member.UiTheme.ShouldBe(UiThemes.Steam);
    }

    [Fact]
    public async Task PutUiTheme_ReturnsValidationProblem_ForUnknownTheme()
    {
        // Arrange
        await LinkMemberAsync(AuthTestHelper.AllowedDiscordUserId, "ShrimpPoboy");
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/members/me/ui-theme",
            new PutMemberUiThemeRequest("neon-punk"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutUiTheme_Returns403_WhenNotAllowlisted()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.DeniedDiscordUserId);

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/members/me/ui-theme",
            new PutMemberUiThemeRequest(UiThemes.FrutigerAero));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetCurrentMember_ReturnsSavedTheme_AfterPut()
    {
        // Arrange
        await LinkMemberAsync(AuthTestHelper.AllowedDiscordUserId, "ShrimpPoboy");
        var client = _factory.CreateAuthenticatedClient();
        await client.PutAsJsonAsync(
            "/api/members/me/ui-theme",
            new PutMemberUiThemeRequest(UiThemes.FrutigerAero));

        // Act
        var member = await client.GetFromJsonAsync<CurrentMemberDto>("/api/members/me");

        // Assert
        member.ShouldNotBeNull();
        member.UiTheme.ShouldBe(UiThemes.FrutigerAero);
    }

    private async Task LinkMemberAsync(string discordId, string raUsername)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var member = await db.Members.SingleAsync(m => m.RaUsername == raUsername);
        member.DiscordId = discordId;
        await db.SaveChangesAsync();
    }
}
