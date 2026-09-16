using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Search;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class SearchApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public SearchApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetSearch_FindsGameMemberAndLeaderboard()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var game = new Game
            {
                RaGameId = 9001,
                Title = "Searchable Adventure",
                ConsoleId = 1,
                ConsoleName = "NES"
            };
            db.Games.Add(game);
            await db.SaveChangesAsync();
            db.Leaderboards.Add(new Leaderboard
            {
                GameId = game.Id,
                RaLeaderboardId = 88001,
                Title = "Searchable Board"
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var response = await client.GetFromJsonAsync<SearchResponse>("/api/search?q=Searchable");

        // Assert
        response.ShouldNotBeNull();
        response.Hits.Count.ShouldBeGreaterThanOrEqualTo(2);
        response.Hits.Any(h => h.Kind == "Game").ShouldBeTrue();
        response.Hits.Any(h => h.Kind == "Leaderboard").ShouldBeTrue();
    }
}
