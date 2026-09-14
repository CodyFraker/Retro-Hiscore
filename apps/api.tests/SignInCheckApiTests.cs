using System.Net;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class SignInCheckApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public SignInCheckApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetSignInCheck_WithoutServiceKey_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            $"/api/auth/sign-in-check?discordId={AuthTestHelper.AllowedDiscordUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSignInCheck_WithServiceKey_ReturnsNoContent_WhenInvited()
    {
        // Arrange
        var client = _factory.CreateSignInCheckClient();

        // Act
        var response = await client.GetAsync(
            $"/api/auth/sign-in-check?discordId={AuthTestHelper.AllowedDiscordUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetSignInCheck_WithServiceKey_ReturnsNotFound_WhenNotInvited()
    {
        // Arrange
        var client = _factory.CreateSignInCheckClient();

        // Act
        var response = await client.GetAsync(
            $"/api/auth/sign-in-check?discordId={AuthTestHelper.DeniedDiscordUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
