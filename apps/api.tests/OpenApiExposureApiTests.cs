using System.Net;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class OpenApiExposureApiTests
{
    private readonly ApiFactory _factory;

    public OpenApiExposureApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetOpenApi_InTesting_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/openapi/v1.json");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
