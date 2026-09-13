using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Rivalry;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class RivalryApiTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public RivalryApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetRivalry_ReturnsNotFound_WhenMemberMissing()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/rivalry/ShrimpPoboy/nobody");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRivalry_ReturnsLeadCounts()
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
                RaLeaderboardId = 3813030,
                GameId = game.Id,
                Title = "Board A",
                Format = "VALUE",
                RankAsc = false
            };
            var boardB = new Leaderboard
            {
                RaLeaderboardId = 3813031,
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
                    MemberId = billy.Id,
                    Score = 90,
                    FormattedScore = "90",
                    FriendRank = 1,
                    SyncedAt = DateTimeOffset.UtcNow
                });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var rivalry = await client.GetFromJsonAsync<RivalryResponse>(
            "/api/rivalry/ShrimpPoboy/beefboybilly",
            JsonOptions);

        // Assert
        rivalry.ShouldNotBeNull();
        rivalry.MemberALeads.ShouldBe(1);
        rivalry.MemberBLeads.ShouldBe(1);
        rivalry.Games.Count.ShouldBe(1);
        rivalry.Games[0].Boards.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetRivalry_CountsTiedBoards_WhenBothRankOne()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var shrimp = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            var billy = await db.Members.SingleAsync(m => m.RaUsername == "beefboybilly");
            var game = await db.Games.SingleAsync(g => g.RaGameId == 38130);
            var board = new Leaderboard
            {
                RaLeaderboardId = 3813032,
                GameId = game.Id,
                Title = "Tied Board",
                Format = "VALUE",
                RankAsc = false
            };
            db.Leaderboards.Add(board);
            await db.SaveChangesAsync();

            db.LeaderboardEntries.AddRange(
                new LeaderboardEntry
                {
                    LeaderboardId = board.Id,
                    MemberId = shrimp.Id,
                    Score = 100,
                    FormattedScore = "100",
                    FriendRank = 1,
                    SyncedAt = DateTimeOffset.UtcNow
                },
                new LeaderboardEntry
                {
                    LeaderboardId = board.Id,
                    MemberId = billy.Id,
                    Score = 100,
                    FormattedScore = "100",
                    FriendRank = 1,
                    SyncedAt = DateTimeOffset.UtcNow
                });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var rivalry = await client.GetFromJsonAsync<RivalryResponse>(
            "/api/rivalry/ShrimpPoboy/beefboybilly",
            JsonOptions);

        // Assert
        rivalry.ShouldNotBeNull();
        rivalry.TiedBoards.ShouldBe(1);
        rivalry.MemberALeads.ShouldBe(0);
        rivalry.MemberBLeads.ShouldBe(0);
    }
}
