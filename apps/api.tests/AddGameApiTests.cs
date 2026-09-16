using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RetroHiscore.Api.Features.Admin;
using RetroHiscore.Api.Features.Ra;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class AddGameApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public AddGameApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _factory.RaApiClient.ClearReceivedCalls();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PostAdminGame_ReturnsConflict()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/games", new PostAdminGameRequest(99999));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        await _factory.RaApiClient.DidNotReceiveWithAnyArgs().GetGameAsync(default, default, default);
    }
}
