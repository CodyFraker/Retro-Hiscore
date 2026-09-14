using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Members;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class MemberProfileApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public MemberProfileApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PutApiKey_SetsKeyOnLinkedMember()
    {
        // Arrange
        await LinkMemberAsync(AuthTestHelper.AllowedDiscordUserId, "ShrimpPoboy");
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.PutAsJsonAsync("/api/members/me/api-key", new PutMemberApiKeyRequest("member-key-123"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
        member.RaApiKey.ShouldBe("member-key-123");
    }

    [Fact]
    public async Task PutApiKey_Returns404_WhenDiscordNotLinked()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.PutAsJsonAsync("/api/members/me/api-key", new PutMemberApiKeyRequest("member-key-123"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutApiKey_Returns403_WhenNotAllowlisted()
    {
        // Arrange
        await LinkMemberAsync(AuthTestHelper.DeniedDiscordUserId, "beefboybilly");
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.DeniedDiscordUserId);

        // Act
        var response = await client.PutAsJsonAsync("/api/members/me/api-key", new PutMemberApiKeyRequest("member-key-123"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PutProfile_UpdatesAvatarUrl()
    {
        // Arrange
        await LinkMemberAsync(AuthTestHelper.AllowedDiscordUserId, "ShrimpPoboy");
        var client = _factory.CreateAuthenticatedClient();
        const string avatarUrl = "https://cdn.discordapp.com/avatars/1/abc.png";

        // Act
        var response = await client.PutAsJsonAsync("/api/members/me/profile", new PutMemberProfileRequest(avatarUrl));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
        member.AvatarUrl.ShouldBe(avatarUrl);
    }

    [Fact]
    public async Task GetMembersMe_ReturnsLinkedMember_WithoutApiKey()
    {
        // Arrange
        await LinkMemberAsync(AuthTestHelper.AllowedDiscordUserId, "ShrimpPoboy", avatarUrl: "https://cdn.discordapp.com/a.png", apiKey: "secret");
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/members/me");
        var member = await response.Content.ReadFromJsonAsync<MemberDto>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        member.ShouldNotBeNull();
        member.RaUsername.ShouldBe("ShrimpPoboy");
        member.AvatarUrl.ShouldBe("https://cdn.discordapp.com/a.png");
        member.HasApiKey.ShouldBeTrue();
    }

    private async Task LinkMemberAsync(
        string discordId,
        string raUsername,
        string? avatarUrl = null,
        string? apiKey = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var member = await db.Members.SingleAsync(m => m.RaUsername == raUsername);
        member.DiscordId = discordId;
        member.AvatarUrl = avatarUrl;
        member.RaApiKey = apiKey;
        await db.SaveChangesAsync();
    }
}
