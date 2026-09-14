using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Ra;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class LeaderboardPopulationHistoryApiTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public LeaderboardPopulationHistoryApiTests(ApiFactory factory)
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
    public async Task GetGameLeaderboardPopulationHistory_ReturnsSeriesForTrackedGame()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var game = await db.Games.FirstAsync(g => g.RaGameId == 38130);
        var leaderboard = new Leaderboard
        {
            GameId = game.Id,
            RaLeaderboardId = 3813010,
            Title = "High Score",
            Format = "VALUE",
            RankAsc = false
        };
        db.Leaderboards.Add(leaderboard);
        await db.SaveChangesAsync();
        var syncedAt = DateTimeOffset.Parse("2026-02-01T12:00:00Z");
        leaderboard.GlobalEntryCount = 500;
        leaderboard.GlobalEntryCountSyncedAt = syncedAt;
        db.LeaderboardPopulationSnapshots.Add(new LeaderboardPopulationSnapshot
        {
            LeaderboardId = leaderboard.Id,
            EntryCount = 500,
            SyncedAt = syncedAt
        });
        await db.SaveChangesAsync();

        // Act
        var response = await client.GetFromJsonAsync<GameLeaderboardPopulationHistoryResponse>(
            "/api/games/38130/leaderboard-population-history",
            JsonOptions);

        // Assert
        response.ShouldNotBeNull();
        response.Boards.ShouldNotBeEmpty();
        var series = response.Boards.First(b => b.RaLeaderboardId == leaderboard.RaLeaderboardId);
        series.Points.Count.ShouldBe(1);
        series.Points[0].EntryCount.ShouldBe(500);
    }

    [Fact]
    public async Task GetGameLeaderboards_IncludesGlobalEntryCount()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var game = await db.Games.FirstAsync(g => g.RaGameId == 38130);
        var leaderboard = new Leaderboard
        {
            GameId = game.Id,
            RaLeaderboardId = 3813011,
            Title = "High Score",
            Format = "VALUE",
            RankAsc = false,
            GlobalEntryCount = 10247
        };
        db.Leaderboards.Add(leaderboard);
        leaderboard.GlobalEntryCountSyncedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();

        // Act
        var detail = await client.GetFromJsonAsync<GameLeaderboardsResponse>(
            "/api/games/38130/leaderboards",
            JsonOptions);

        // Assert
        detail.ShouldNotBeNull();
        var board = detail.Leaderboards.First(l => l.RaLeaderboardId == leaderboard.RaLeaderboardId);
        board.GlobalEntryCount.ShouldBe(10247);
    }
}
