using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Admin;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class AdminGamesApiTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public AdminGamesApiTests(ApiFactory factory)
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
    public async Task GetAdminGames_ReturnsTrackedGames_ForAdmin()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.GetAsync("/api/admin/games");
        var games = await response.Content.ReadFromJsonAsync<List<AdminGameDto>>(JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        games.ShouldNotBeNull();
        games.Count.ShouldBeGreaterThan(0);
        games.ShouldAllBe(g => g.SourceCount >= 0);
    }

    [Fact]
    public async Task GetAdminGames_ReturnsForbidden_ForNonAdmin()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var response = await client.GetAsync("/api/admin/games");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostAdminGame_RefreshAndDelete_WorkForAdmin()
    {
        // Arrange
        const int raGameId = 77777;
        await _factory.SetMemberApiKeyAsync("ShrimpPoboy", "shrimp-test-key");
        SetupRaMocks(raGameId, includeMemberScore: false);
        var admin = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act — add
        var addResponse = await admin.PostAsJsonAsync("/api/admin/games", new PostAdminGameRequest(raGameId));
        var created = await addResponse.Content.ReadFromJsonAsync<AdminGameDto>(JsonOptions);

        // Assert — add
        addResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        created.ShouldNotBeNull();
        created!.RaGameId.ShouldBe(raGameId);
        created.SourceCount.ShouldBe(0);

        // Act — refresh
        var refreshResponse = await admin.PostAsync($"/api/admin/games/{raGameId}/refresh", null);
        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<AdminGameDto>(JsonOptions);

        // Assert — refresh
        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        refreshed.ShouldNotBeNull();
        refreshed!.MetadataSyncedAt.ShouldNotBeNull();

        // Act — delete
        var deleteResponse = await admin.DeleteAsync($"/api/admin/games/{raGameId}");

        // Assert — delete
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Games.AnyAsync(g => g.RaGameId == raGameId)).ShouldBeFalse();
    }

    private void SetupRaMocks(int raGameId, bool includeMemberScore)
    {
        _factory.RaApiClient
            .GetGameAsync(raGameId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new RaGameDto
            {
                Title = "Admin Test Game",
                ConsoleId = 3,
                ConsoleName = "SNES",
                ImageBoxArt = "/Images/box-admin.png"
            }));

        _factory.RaApiClient
            .GetGameLeaderboardsAsync(raGameId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaGameLeaderboardDto>>(
            [
                new RaGameLeaderboardDto
                {
                    Id = raGameId * 10L,
                    Title = "High Score",
                    Format = "VALUE",
                    RankAsc = false
                }
            ]));

        _factory.RaApiClient
            .GetUserGameLeaderboardsAsync(raGameId, Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaUserGameLeaderboardDto>>([]));

        _factory.RaApiClient
            .GetConsoleIdsAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaConsoleIdDto>>(
            [
                new RaConsoleIdDto { Id = 3, Name = "SNES", IconUrl = "https://example.com/snes.png" }
            ]));
    }
}
