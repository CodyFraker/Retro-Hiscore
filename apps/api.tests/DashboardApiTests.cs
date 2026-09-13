using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Dashboard;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class DashboardApiTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public DashboardApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetDashboard_ReturnsChampionshipOrderedByFriendRankOnes()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var shrimp = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            var billy = await db.Members.SingleAsync(m => m.RaUsername == "beefboybilly");
            var game = await db.Games.SingleAsync(g => g.RaGameId == 38130);
            var boardA = new Leaderboard
            {
                RaLeaderboardId = 3813010,
                GameId = game.Id,
                Title = "Board A",
                Format = "VALUE",
                RankAsc = false
            };
            var boardB = new Leaderboard
            {
                RaLeaderboardId = 3813011,
                GameId = game.Id,
                Title = "Board B",
                Format = "VALUE",
                RankAsc = false
            };
            db.Leaderboards.AddRange(boardA, boardB);
            await db.SaveChangesAsync();

            db.LeaderboardEntries.AddRange(
                new LeaderboardEntry
                {
                    LeaderboardId = boardA.Id,
                    MemberId = shrimp.Id,
                    Score = 100,
                    FormattedScore = "100",
                    FriendRank = 1,
                    SyncedAt = DateTimeOffset.UtcNow
                },
                new LeaderboardEntry
                {
                    LeaderboardId = boardB.Id,
                    MemberId = shrimp.Id,
                    Score = 90,
                    FormattedScore = "90",
                    FriendRank = 1,
                    SyncedAt = DateTimeOffset.UtcNow
                },
                new LeaderboardEntry
                {
                    LeaderboardId = boardA.Id,
                    MemberId = billy.Id,
                    Score = 50,
                    FormattedScore = "50",
                    FriendRank = 2,
                    SyncedAt = DateTimeOffset.UtcNow
                });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var dashboard = await client.GetFromJsonAsync<DashboardResponse>("/api/dashboard", JsonOptions);

        // Assert
        dashboard.ShouldNotBeNull();
        dashboard.Championship.Count.ShouldBeGreaterThanOrEqualTo(2);
        dashboard.Championship[0].RaUsername.ShouldBe("ShrimpPoboy");
        dashboard.Championship[0].FriendRankOnes.ShouldBe(2);
        dashboard.Championship[0].BoardsWithScore.ShouldBe(2);
    }

    [Fact]
    public async Task GetDashboard_ReturnsActivityAcrossTwoSyncs()
    {
        // Arrange
        var olderSync = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var newerSync = new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var shrimp = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            var game = await db.Games.SingleAsync(g => g.RaGameId == 38130);
            var board = new Leaderboard
            {
                RaLeaderboardId = 3813012,
                GameId = game.Id,
                Title = "Board C",
                Format = "VALUE",
                RankAsc = false
            };
            db.Leaderboards.Add(board);
            await db.SaveChangesAsync();

            db.LeaderboardEntrySnapshots.AddRange(
                new LeaderboardEntrySnapshot
                {
                    LeaderboardId = board.Id,
                    MemberId = shrimp.Id,
                    Score = 100,
                    FormattedScore = "100",
                    FriendRank = 2,
                    SyncedAt = olderSync
                },
                new LeaderboardEntrySnapshot
                {
                    LeaderboardId = board.Id,
                    MemberId = shrimp.Id,
                    Score = 200,
                    FormattedScore = "200",
                    FriendRank = 1,
                    SyncedAt = newerSync
                });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var dashboard = await client.GetFromJsonAsync<DashboardResponse>("/api/dashboard", JsonOptions);

        // Assert
        dashboard.ShouldNotBeNull();
        dashboard.Activity.Count.ShouldBe(1);
        dashboard.Activity[0].RaUsername.ShouldBe("ShrimpPoboy");
        dashboard.Activity[0].ScoreDelta.ShouldBe(100);
        dashboard.Activity[0].FriendRankDelta.ShouldBe(1);
        dashboard.Activity[0].RaLeaderboardId.ShouldBe(3813012);
    }

    [Fact]
    public async Task GetDashboard_ReturnsEnrichedGameFields()
    {
        // Arrange
        var activityAt = new DateTimeOffset(2026, 2, 1, 12, 0, 0, TimeSpan.Zero);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var shrimp = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            var game = await db.Games.SingleAsync(g => g.RaGameId == 38130);
            game.Title = "Pinball";
            var board = new Leaderboard
            {
                RaLeaderboardId = 3813013,
                GameId = game.Id,
                Title = "High Score",
                Format = "VALUE",
                RankAsc = false
            };
            db.Leaderboards.Add(board);
            await db.SaveChangesAsync();

            db.LeaderboardEntries.Add(new LeaderboardEntry
            {
                LeaderboardId = board.Id,
                MemberId = shrimp.Id,
                Score = 500,
                FormattedScore = "500",
                FriendRank = 1,
                ScoreUpdatedAt = activityAt,
                SyncedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var dashboard = await client.GetFromJsonAsync<DashboardResponse>("/api/dashboard", JsonOptions);

        // Assert
        dashboard.ShouldNotBeNull();
        var pinball = dashboard.Games.Single(g => g.RaGameId == 38130);
        pinball.Title.ShouldBe("Pinball");
        pinball.FriendRankOneLeader.ShouldNotBeNull();
        pinball.FriendRankOneLeader!.DisplayName.ShouldBe("ShrimpPoboy");
        pinball.FriendRankOneLeader.FriendRankOnes.ShouldBe(1);
        pinball.LastActivityAt.ShouldBe(activityAt);
    }

    [Fact]
    public async Task GetDashboard_OrdersGamesByLastActivityThenTitle()
    {
        var olderActivity = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var newerActivity = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var shrimp = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");

            var olderGame = new Game
            {
                RaGameId = 88001,
                Title = "Alpha Game"
            };
            var newerGame = new Game
            {
                RaGameId = 88002,
                Title = "Zulu Game"
            };
            db.Games.AddRange(olderGame, newerGame);
            await db.SaveChangesAsync();

            var olderBoard = new Leaderboard
            {
                RaLeaderboardId = 880010,
                GameId = olderGame.Id,
                Title = "Older Board",
                Format = "VALUE",
                RankAsc = false
            };
            var newerBoard = new Leaderboard
            {
                RaLeaderboardId = 880020,
                GameId = newerGame.Id,
                Title = "Newer Board",
                Format = "VALUE",
                RankAsc = false
            };
            db.Leaderboards.AddRange(olderBoard, newerBoard);
            await db.SaveChangesAsync();

            db.LeaderboardEntries.AddRange(
                new LeaderboardEntry
                {
                    LeaderboardId = olderBoard.Id,
                    MemberId = shrimp.Id,
                    Score = 10,
                    FormattedScore = "10",
                    FriendRank = 1,
                    ScoreUpdatedAt = olderActivity,
                    SyncedAt = DateTimeOffset.UtcNow
                },
                new LeaderboardEntry
                {
                    LeaderboardId = newerBoard.Id,
                    MemberId = shrimp.Id,
                    Score = 20,
                    FormattedScore = "20",
                    FriendRank = 1,
                    ScoreUpdatedAt = newerActivity,
                    SyncedAt = DateTimeOffset.UtcNow
                });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        var dashboard = await client.GetFromJsonAsync<DashboardResponse>("/api/dashboard", JsonOptions);

        dashboard.ShouldNotBeNull();
        var orderedIds = dashboard.Games
            .Where(g => g.RaGameId is 88001 or 88002)
            .Select(g => g.RaGameId)
            .ToList();
        orderedIds.ShouldBe([88002, 88001]);
    }
}
