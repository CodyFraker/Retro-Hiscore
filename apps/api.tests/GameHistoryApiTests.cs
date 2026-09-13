using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Games;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class GameHistoryApiTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public GameHistoryApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetGameHistory_Returns404_WhenGameNotTracked()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/games/99999/history");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetGameHistory_ReturnsItemsOrderedBySyncedAtDesc()
    {
        // Arrange
        var olderSync = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var newerSync = new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            var game = await db.Games.SingleAsync(g => g.RaGameId == 38130);
            var boardA = new Leaderboard
            {
                RaLeaderboardId = 3813004,
                GameId = game.Id,
                Title = "Board A",
                Format = "VALUE",
                RankAsc = false
            };
            var boardB = new Leaderboard
            {
                RaLeaderboardId = 3813005,
                GameId = game.Id,
                Title = "Board B",
                Format = "VALUE",
                RankAsc = false
            };
            db.Leaderboards.AddRange(boardA, boardB);
            await db.SaveChangesAsync();

            db.LeaderboardEntrySnapshots.AddRange(
                new LeaderboardEntrySnapshot
                {
                    LeaderboardId = boardA.Id,
                    MemberId = member.Id,
                    Score = 100,
                    FormattedScore = "100",
                    FriendRank = 1,
                    SyncedAt = olderSync
                },
                new LeaderboardEntrySnapshot
                {
                    LeaderboardId = boardB.Id,
                    MemberId = member.Id,
                    Score = 200,
                    FormattedScore = "200",
                    FriendRank = 2,
                    SyncedAt = newerSync
                },
                new LeaderboardEntrySnapshot
                {
                    LeaderboardId = boardA.Id,
                    MemberId = member.Id,
                    Score = 150,
                    FormattedScore = "150",
                    FriendRank = 1,
                    SyncedAt = newerSync
                });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var history = await client.GetFromJsonAsync<GameHistoryResponse>(
            "/api/games/38130/history",
            JsonOptions);

        // Assert
        history.ShouldNotBeNull();
        history.Total.ShouldBe(3);
        history.Items.Count.ShouldBe(3);
        history.Items[0].SyncedAt.ShouldBe(newerSync);
        history.Items[0].LeaderboardTitle.ShouldBe("Board A");
        history.Items[0].RaUsername.ShouldBe("ShrimpPoboy");
        history.Items[1].SyncedAt.ShouldBe(newerSync);
        history.Items[1].LeaderboardTitle.ShouldBe("Board B");
        history.Items[2].SyncedAt.ShouldBe(olderSync);
        history.Items[2].LeaderboardTitle.ShouldBe("Board A");
    }

    [Fact]
    public async Task GetGameHistory_RespectsPagination()
    {
        // Arrange
        var sync = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            var game = await db.Games.SingleAsync(g => g.RaGameId == 38130);
            var board = new Leaderboard
            {
                RaLeaderboardId = 3813006,
                GameId = game.Id,
                Title = "Board C",
                Format = "VALUE",
                RankAsc = false
            };
            db.Leaderboards.Add(board);
            await db.SaveChangesAsync();

            for (var i = 0; i < 3; i++)
            {
                db.LeaderboardEntrySnapshots.Add(new LeaderboardEntrySnapshot
                {
                    LeaderboardId = board.Id,
                    MemberId = member.Id,
                    Score = 100 + i,
                    FormattedScore = $"{100 + i}",
                    FriendRank = 1,
                    SyncedAt = sync.AddHours(i)
                });
            }

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var history = await client.GetFromJsonAsync<GameHistoryResponse>(
            "/api/games/38130/history?limit=2&offset=1",
            JsonOptions);

        // Assert
        history.ShouldNotBeNull();
        history.Total.ShouldBe(3);
        history.Limit.ShouldBe(2);
        history.Offset.ShouldBe(1);
        history.Items.Count.ShouldBe(2);
    }
}
