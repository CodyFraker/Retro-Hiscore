using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class DeleteGameApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public DeleteGameApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task DeleteGame_ReturnsNotFound_WhenMissing()
    {
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.DeleteAsync("/api/games/999999");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteGame_RemovesGameAndLeaderboards()
    {
        const int raGameId = 38130;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var game = await db.Games.SingleAsync(g => g.RaGameId == raGameId);
            var board = new Leaderboard
            {
                RaLeaderboardId = 3813099,
                GameId = game.Id,
                Title = "Test Board",
                Format = "VALUE",
                RankAsc = false
            };
            db.Leaderboards.Add(board);
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        var response = await client.DeleteAsync($"/api/games/{raGameId}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.Games.AnyAsync(g => g.RaGameId == raGameId)).ShouldBeFalse();
            (await db.Leaderboards.AnyAsync(l => l.RaLeaderboardId == 3813099)).ShouldBeFalse();
        }
    }
}
