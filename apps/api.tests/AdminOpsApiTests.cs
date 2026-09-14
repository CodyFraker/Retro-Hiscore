using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RetroHiscore.Api.Features.Admin;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class AdminOpsApiTests
{
    private readonly ApiFactory _factory;

    public AdminOpsApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAdminOps_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/admin/ops");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAdminOps_WithNonAdminAllowlistedToken_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            AuthTestHelper.CreateToken(AuthTestHelper.SecondAllowedDiscordUserId));

        // Act
        var response = await client.GetAsync("/api/admin/ops");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAdminOps_WithAdminToken_ReturnsOkWithExpectedSections()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.GetAsync("/api/admin/ops");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AdminOpsDto>();
        body.ShouldNotBeNull();
        body.Health.OverallStatus.ShouldNotBeNullOrWhiteSpace();
        body.MemberCoverageSummary.TotalMembers.ShouldBeGreaterThan(0);
        body.Members.Count.ShouldBe(body.MemberCoverageSummary.TotalMembers);
        body.Config.KeysInPool.ShouldBeGreaterThan(0);
        body.Config.SharedCatalogKeyConfigured.ShouldBeTrue();
    }
}
