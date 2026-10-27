using System.Net;
using System.Net.Http.Headers;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class AuthJwtValidationApiTests
{
    private readonly ApiFactory _factory;

    public AuthJwtValidationApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCurrentMember_WithWrongAudienceToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            AuthTestHelper.CreateTokenWithWrongAudience(AuthTestHelper.AllowedDiscordUserId));

        // Act
        var response = await client.GetAsync("/api/members/me");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
