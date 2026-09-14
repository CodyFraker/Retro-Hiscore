using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Members;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class PutMemberRaAccountApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public PutMemberRaAccountApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var admin = await db.Members.SingleAsync(m => m.DiscordId == AuthTestHelper.AdminDiscordUserId);
        admin.RaUsername = null;
        admin.RaApiKey = null;
        await db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PutRaAccount_SetsUsernameAndApiKey()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/members/me/ra-account",
            new PutMemberRaAccountRequest("ShrimpPoboy", "member-key-123"));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var member = await db.Members.SingleAsync(m => m.DiscordId == AuthTestHelper.AllowedDiscordUserId);
        member.RaUsername.ShouldBe("ShrimpPoboy");
        member.RaApiKey.ShouldBe("member-key-123");
    }
}
