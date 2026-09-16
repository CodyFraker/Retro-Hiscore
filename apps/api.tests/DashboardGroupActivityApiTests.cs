using System.Net.Http.Json;
using RetroHiscore.Api.Features.Dashboard;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class DashboardGroupActivityApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public DashboardGroupActivityApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetDashboardGroupActivity_WhenNoSnapshots_ReturnsWindowUnavailable()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var response = await client.GetFromJsonAsync<DashboardGroupActivityResponse>("/api/dashboard/group-activity");

        // Assert
        response.ShouldNotBeNull();
        response.WindowUnavailable.ShouldBeTrue();
        response.Items.ShouldBeEmpty();
    }
}
