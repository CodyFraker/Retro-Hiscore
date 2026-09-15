using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Dashboard;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class DashboardApiTests : IAsyncLifetime
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
    public async Task GetDashboard_ReturnsRecentGroupGamesAggregatedByRaGameId()
    {
        // Arrange
        var olderPlay = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var newerPlay = new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var shrimp = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            var billy = await db.Members.SingleAsync(m => m.RaUsername == "beefboybilly");

            db.MemberRecentGamePlays.AddRange(
                new MemberRecentGamePlay
                {
                    MemberId = shrimp.Id,
                    RaGameId = 9001,
                    Title = "Shared Game",
                    ConsoleId = 1,
                    ConsoleName = "Genesis",
                    LastPlayedAt = newerPlay,
                    NumAchieved = 3,
                    NumPossibleAchievements = 10,
                    SyncedAt = newerPlay
                },
                new MemberRecentGamePlay
                {
                    MemberId = billy.Id,
                    RaGameId = 9001,
                    Title = "Shared Game",
                    ConsoleId = 1,
                    ConsoleName = "Genesis",
                    LastPlayedAt = olderPlay,
                    NumAchieved = 1,
                    NumPossibleAchievements = 10,
                    SyncedAt = newerPlay
                },
                new MemberRecentGamePlay
                {
                    MemberId = shrimp.Id,
                    RaGameId = 9002,
                    Title = "Solo Game",
                    ConsoleId = 2,
                    ConsoleName = "SNES",
                    LastPlayedAt = newerPlay.AddDays(1),
                    NumAchieved = 0,
                    NumPossibleAchievements = 0,
                    SyncedAt = newerPlay
                });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var dashboard = await client.GetFromJsonAsync<DashboardResponse>("/api/dashboard", JsonOptions);

        // Assert
        dashboard.ShouldNotBeNull();
        dashboard.RecentGroupGames.Count.ShouldBe(2);
        dashboard.RecentGroupGames[0].RaGameId.ShouldBe(9002);
        dashboard.RecentGroupGames[1].RaGameId.ShouldBe(9001);
        var shared = dashboard.RecentGroupGames.Single(g => g.RaGameId == 9001);
        shared.Players.Count.ShouldBe(2);
        shared.IsTracked.ShouldBeFalse();
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
            shrimp.AvatarUrl = "https://cdn.discordapp.com/avatars/1/test.png";
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
        var games = await client.GetFromJsonAsync<DashboardGamesResponse>(
            "/api/dashboard/games?limit=100",
            JsonOptions);

        // Assert
        games.ShouldNotBeNull();
        var pinball = games.Items.Single(g => g.RaGameId == 38130);
        pinball.Title.ShouldBe("Pinball");
        pinball.FriendRankOneLeader.ShouldNotBeNull();
        pinball.FriendRankOneLeader!.DisplayName.ShouldBe("ShrimpPoboy");
        pinball.FriendRankOneLeader.FriendRankOnes.ShouldBe(1);
        pinball.PlayersWithAvatars.Count.ShouldBe(1);
        pinball.PlayersWithAvatars[0].RaUsername.ShouldBe("ShrimpPoboy");
        pinball.PlayersWithAvatars[0].AvatarUrl.ShouldBe("https://cdn.discordapp.com/avatars/1/test.png");
        pinball.LastActivityAt.ShouldBe(activityAt);
    }

    [Fact]
    public async Task GetDashboardGames_IncludesPlayersEngagedViaAchievementsOnly()
    {
        // Arrange
        const int raGameId = 87001;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var shrimp = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            shrimp.AvatarUrl = "https://cdn.discordapp.com/avatars/1/ach-only.png";
            db.Games.Add(new Game
            {
                RaGameId = raGameId,
                Title = "Achievements Only Game"
            });
            db.RaAchievements.Add(new RaAchievement
            {
                RaAchievementId = 870010,
                RaGameId = raGameId,
                Title = "First trophy",
                Points = 5,
                TrueRatio = 10,
                DisplayOrder = 1
            });
            await db.SaveChangesAsync();

            db.MemberRaAchievements.Add(new MemberRaAchievement
            {
                MemberId = shrimp.Id,
                RaAchievementId = 870010,
                DateEarned = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var games = await client.GetFromJsonAsync<DashboardGamesResponse>(
            "/api/dashboard/games?limit=100",
            JsonOptions);

        // Assert
        games.ShouldNotBeNull();
        var game = games!.Items.Single(g => g.RaGameId == raGameId);
        game.PlayersWithAvatars.Count.ShouldBe(1);
        game.PlayersWithAvatars[0].RaUsername.ShouldBe("ShrimpPoboy");
        game.PlayersWithAvatars[0].AvatarUrl.ShouldBe("https://cdn.discordapp.com/avatars/1/ach-only.png");
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

        var games = await client.GetFromJsonAsync<DashboardGamesResponse>(
            "/api/dashboard/games?limit=100&sort=recent",
            JsonOptions);

        games.ShouldNotBeNull();
        var orderedIds = games.Items
            .Where(g => g.RaGameId is 88001 or 88002)
            .Select(g => g.RaGameId)
            .ToList();
        orderedIds.ShouldBe([88002, 88001]);
    }

    [Fact]
    public async Task GetDashboardGames_ReturnsPaginatedSlice()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            for (var i = 0; i < 3; i++)
            {
                db.Games.Add(new Game
                {
                    RaGameId = 99000 + i,
                    Title = $"Paginate Game {i}"
                });
            }

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        var page = await client.GetFromJsonAsync<DashboardGamesResponse>(
            "/api/dashboard/games?limit=2&offset=0&sort=name",
            JsonOptions);

        page.ShouldNotBeNull();
        page.Limit.ShouldBe(2);
        page.Offset.ShouldBe(0);
        page.Total.ShouldBeGreaterThanOrEqualTo(3);
        page.Items.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetDashboardGames_ReturnsTotalRankedEntriesAcrossAllBoardsWithoutFriendEntries()
    {
        const int raGameId = 87100;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var game = new Game
            {
                RaGameId = raGameId,
                Title = "Global Entry Sum Game"
            };
            db.Games.Add(game);
            await db.SaveChangesAsync();

            db.Leaderboards.AddRange(
                new Leaderboard
                {
                    RaLeaderboardId = 871001,
                    GameId = game.Id,
                    Title = "Board One",
                    Format = "VALUE",
                    RankAsc = false,
                    GlobalEntryCount = 100
                },
                new Leaderboard
                {
                    RaLeaderboardId = 871002,
                    GameId = game.Id,
                    Title = "Board Two",
                    Format = "VALUE",
                    RankAsc = false,
                    GlobalEntryCount = 200
                });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        var games = await client.GetFromJsonAsync<DashboardGamesResponse>(
            "/api/dashboard/games?limit=100",
            JsonOptions);

        games.ShouldNotBeNull();
        var item = games!.Items.Single(g => g.RaGameId == raGameId);
        item.TotalRankedEntriesAcrossBoards.ShouldBe(300);
    }

    [Fact]
    public async Task GetDashboardGames_ReturnsHotAndColdLeaderboardSyncStatus()
    {
        // Arrange
        const int hotRaGameId = 38130;
        const int coldRaGameId = 88002;
        var recentPlay = DateTimeOffset.UtcNow.AddHours(-2);
        var stalePlay = DateTimeOffset.UtcNow.AddDays(-30);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var shrimp = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            db.Games.Add(new Game
            {
                RaGameId = coldRaGameId,
                Title = "Cold Sync Game"
            });
            db.MemberRecentGamePlays.AddRange(
                new MemberRecentGamePlay
                {
                    MemberId = shrimp.Id,
                    RaGameId = hotRaGameId,
                    Title = "Hot Game",
                    ConsoleId = 1,
                    ConsoleName = "Genesis",
                    LastPlayedAt = recentPlay,
                    NumAchieved = 1,
                    NumPossibleAchievements = 10,
                    SyncedAt = recentPlay
                },
                new MemberRecentGamePlay
                {
                    MemberId = shrimp.Id,
                    RaGameId = coldRaGameId,
                    Title = "Cold Sync Game",
                    ConsoleId = 1,
                    ConsoleName = "Genesis",
                    LastPlayedAt = stalePlay,
                    NumAchieved = 0,
                    NumPossibleAchievements = 10,
                    SyncedAt = stalePlay
                });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var games = await client.GetFromJsonAsync<DashboardGamesResponse>(
            "/api/dashboard/games?limit=100",
            JsonOptions);

        // Assert
        games.ShouldNotBeNull();
        var hot = games!.Items.Single(g => g.RaGameId == hotRaGameId);
        hot.LeaderboardSyncStatus.Tier.ShouldBe("Hot");
        hot.LeaderboardSyncStatus.LeaderboardSyncIntervalMinutes.ShouldBe(15);
        hot.LeaderboardSyncStatus.GroupLastPlayedAt.ShouldNotBeNull();

        var cold = games.Items.Single(g => g.RaGameId == coldRaGameId);
        cold.LeaderboardSyncStatus.Tier.ShouldBe("Cold");
        cold.LeaderboardSyncStatus.LeaderboardSyncIntervalMinutes.ShouldBe(1440);
    }
}
