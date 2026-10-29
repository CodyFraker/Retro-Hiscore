using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Dashboard;
using RetroHiscore.Api.Features.Members;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class FriendTrackerApiTests : IAsyncLifetime
{
    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public FriendTrackerApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetMemberChase_ReturnsRankTwoBoardsWithScoreGapForHighScoreBoard()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var viewer = await db.Members.SingleAsync(m => m.DiscordId == AuthTestHelper.AllowedDiscordUserId);
            var leader = await db.Members.SingleAsync(m => m.RaUsername == "beefboybilly");
            var game = await db.Games.SingleAsync(g => g.RaGameId == 38130);
            var board = new Leaderboard
            {
                RaLeaderboardId = 3813099,
                GameId = game.Id,
                Title = "High chase",
                RankAsc = false
            };
            db.Leaderboards.Add(board);
            await db.SaveChangesAsync();

            db.LeaderboardEntries.AddRange(
                new LeaderboardEntry
                {
                    LeaderboardId = board.Id,
                    MemberId = leader.Id,
                    Score = 500,
                    FormattedScore = "500",
                    FriendRank = 1,
                    ScoreUpdatedAt = DateTimeOffset.UtcNow,
                    SyncedAt = DateTimeOffset.UtcNow
                },
                new LeaderboardEntry
                {
                    LeaderboardId = board.Id,
                    MemberId = viewer.Id,
                    Score = 450,
                    FormattedScore = "450",
                    FriendRank = 2,
                    SyncedAt = DateTimeOffset.UtcNow
                });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var chase = await client.GetFromJsonAsync<MemberChaseResponse>("/api/members/me/chase", JsonOptions);

        // Assert
        chase.ShouldNotBeNull();
        var row = chase.Items.Single(i => i.RaLeaderboardId == 3813099);
        row.ScoreGap.ShouldBe(50);
        row.RankAsc.ShouldBeFalse();
        row.LeaderRaUsername.ShouldBe("beefboybilly");
    }

    [Fact]
    public async Task GetMemberChase_UsesAscendingGapWhenRankAsc()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var viewer = await db.Members.SingleAsync(m => m.DiscordId == AuthTestHelper.AllowedDiscordUserId);
            var leader = await db.Members.SingleAsync(m => m.RaUsername == "beefboybilly");
            var game = await db.Games.SingleAsync(g => g.RaGameId == 38130);
            var board = new Leaderboard
            {
                RaLeaderboardId = 3813098,
                GameId = game.Id,
                Title = "Time chase",
                RankAsc = true
            };
            db.Leaderboards.Add(board);
            await db.SaveChangesAsync();

            db.LeaderboardEntries.AddRange(
                new LeaderboardEntry
                {
                    LeaderboardId = board.Id,
                    MemberId = leader.Id,
                    Score = 100,
                    FormattedScore = "1:40",
                    FriendRank = 1,
                    SyncedAt = DateTimeOffset.UtcNow
                },
                new LeaderboardEntry
                {
                    LeaderboardId = board.Id,
                    MemberId = viewer.Id,
                    Score = 130,
                    FormattedScore = "2:10",
                    FriendRank = 2,
                    SyncedAt = DateTimeOffset.UtcNow
                });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var chase = await client.GetFromJsonAsync<MemberChaseResponse>("/api/members/me/chase", JsonOptions);

        // Assert
        chase.ShouldNotBeNull();
        chase.Items.Single(i => i.RaLeaderboardId == 3813098).ScoreGap.ShouldBe(30);
    }

    [Fact]
    public async Task PutMemberPin_ThenGetMemberPins_ReturnsPinnedGame()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var put = await client.PutAsync("/api/members/me/pins/38130", null);
        var pins = await client.GetFromJsonAsync<MemberPinsResponse>("/api/members/me/pins", JsonOptions);

        // Assert
        put.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        pins.ShouldNotBeNull();
        pins.Items.Any(p => p.RaGameId == 38130).ShouldBeTrue();
    }

    [Fact]
    public async Task GetDashboardNowPlaying_FiltersToOnlineMembersWithPresence()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var member = await db.Members.SingleAsync(m => m.DiscordId == AuthTestHelper.AllowedDiscordUserId);
            member.RaStatus = "Online";
            member.RaPresenceRaGameId = 38130;
            member.RaPresenceGameTitle = "Pinball";
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetFromJsonAsync<NowPlayingResponse>("/api/dashboard/now-playing", JsonOptions);

        // Assert
        response.ShouldNotBeNull();
        response.Items.Count.ShouldBe(1);
        response.Items[0].RaGameId.ShouldBe(38130);
        response.Items[0].IsTracked.ShouldBeTrue();
    }

    [Fact]
    public async Task GetDashboardSeason_CountsBoardTakeInUtcMonth()
    {
        // Arrange
        var takeAt = new DateTimeOffset(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var shrimp = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            var game = await db.Games.SingleAsync(g => g.RaGameId == 38130);
            var board = new Leaderboard
            {
                RaLeaderboardId = 3813077,
                GameId = game.Id,
                Title = "Season board",
                RankAsc = false
            };
            db.Leaderboards.Add(board);
            await db.SaveChangesAsync();

            db.SeasonBoardTakes.Add(new SeasonBoardTake
            {
                MemberId = shrimp.Id,
                LeaderboardId = board.Id,
                TakenAt = takeAt
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var season = await client.GetFromJsonAsync<SeasonStandingsResponse>(
            "/api/dashboard/seasons?year=2026&month=3",
            JsonOptions);

        // Assert
        season.ShouldNotBeNull();
        season.Rows.Single(r => r.RaUsername == "ShrimpPoboy").BoardsTaken.ShouldBe(1);
    }
}
