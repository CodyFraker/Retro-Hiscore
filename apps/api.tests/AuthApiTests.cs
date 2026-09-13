using System.Net;
using System.Net.Http.Headers;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class AuthApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public AuthApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetGames_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/games");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetGames_WithValidToken_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/games");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetGames_WithNonAllowlistedToken_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            AuthTestHelper.CreateToken(AuthTestHelper.DeniedDiscordUserId));

        // Act
        var response = await client.GetAsync("/api/games");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
