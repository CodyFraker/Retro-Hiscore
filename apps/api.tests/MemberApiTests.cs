using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Members;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class MemberApiTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public MemberApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetMembers_ReturnsStatsOrderedByFriendRankOnes()
    {
        // Arrange
        Guid shrimpId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var shrimp = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            var billy = await db.Members.SingleAsync(m => m.RaUsername == "beefboybilly");
            shrimpId = shrimp.Id;
            var game = await db.Games.SingleAsync(g => g.RaGameId == 38130);
            var boardA = new Leaderboard
            {
                RaLeaderboardId = 3813020,
                GameId = game.Id,
                Title = "Board A",
                Format = "VALUE",
                RankAsc = false
            };
            var boardB = new Leaderboard
            {
                RaLeaderboardId = 3813021,
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
        var members = await client.GetFromJsonAsync<List<MemberDto>>("/api/members", JsonOptions);

        // Assert
        members.ShouldNotBeNull();
        members[0].Id.ShouldBe(shrimpId);
        members[0].FriendRankOnes.ShouldBe(2);
        members[0].BoardsWithScore.ShouldBe(2);
    }

    [Fact]
    public async Task GetMember_ReturnsNotFound_WhenUsernameMissing()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/members/nobody");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMember_ReturnsAggregates_FromCurrentEntries()
    {
        // Arrange
        Guid memberId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            memberId = member.Id;
            var game = await db.Games.SingleAsync(g => g.RaGameId == 38130);
            var boardA = new Leaderboard
            {
                RaLeaderboardId = 3813001,
                GameId = game.Id,
                Title = "Board A",
                Format = "VALUE",
                RankAsc = false
            };
            var boardB = new Leaderboard
            {
                RaLeaderboardId = 3813002,
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
                    MemberId = memberId,
                    Score = 100,
                    FormattedScore = "100",
                    FriendRank = 1,
                    SyncedAt = DateTimeOffset.UtcNow
                },
                new LeaderboardEntry
                {
                    LeaderboardId = boardB.Id,
                    MemberId = memberId,
                    Score = 50,
                    FormattedScore = "50",
                    FriendRank = 2,
                    SyncedAt = DateTimeOffset.UtcNow
                });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var detail = await client.GetFromJsonAsync<MemberDetailDto>("/api/members/shrimppoboy", JsonOptions);

        // Assert
        detail.ShouldNotBeNull();
        detail.RaUsername.ShouldBe("ShrimpPoboy");
        detail.BoardsWithScore.ShouldBe(2);
        detail.FriendRankOnes.ShouldBe(1);
        detail.Standings.Count.ShouldBe(2);
        detail.Standings.Select(s => s.LeaderboardTitle).ShouldBe(["Board A", "Board B"]);
    }

    [Fact]
    public async Task GetMemberHistory_ReturnsNotFound_WhenUsernameMissing()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/members/nobody/history");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMemberHistory_ReturnsOnlyMemberSnapshots()
    {
        // Arrange
        var olderSync = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var newerSync = new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var shrimp = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            var billy = await db.Members.SingleAsync(m => m.RaUsername == "beefboybilly");
            var game = await db.Games.SingleAsync(g => g.RaGameId == 38130);
            var board = new Leaderboard
            {
                RaLeaderboardId = 3813003,
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
                    FriendRank = 1,
                    SyncedAt = newerSync
                },
                new LeaderboardEntrySnapshot
                {
                    LeaderboardId = board.Id,
                    MemberId = billy.Id,
                    Score = 50,
                    FormattedScore = "50",
                    FriendRank = 2,
                    SyncedAt = newerSync
                });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var history = await client.GetFromJsonAsync<MemberHistoryResponse>(
            "/api/members/ShrimpPoboy/history",
            JsonOptions);

        // Assert
        history.ShouldNotBeNull();
        history.Total.ShouldBe(1);
        history.Items.Count.ShouldBe(1);
        history.Items[0].FormattedScore.ShouldBe("100");
        history.Items[0].FriendRank.ShouldBe(1);
    }

    [Fact]
    public async Task GetMemberHistory_ReturnsItemsOrderedBySyncedAtDesc()
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
        var history = await client.GetFromJsonAsync<MemberHistoryResponse>(
            "/api/members/shrimppoboy/history",
            JsonOptions);

        // Assert
        history.ShouldNotBeNull();
        history.Total.ShouldBe(3);
        history.Items.Count.ShouldBe(3);
        history.Items[0].SyncedAt.ShouldBe(newerSync);
        history.Items[0].LeaderboardTitle.ShouldBe("Board A");
        history.Items[0].RaGameId.ShouldBe(38130);
        history.Items[0].GameTitle.ShouldNotBeNullOrWhiteSpace();
        history.Items[1].SyncedAt.ShouldBe(newerSync);
        history.Items[1].LeaderboardTitle.ShouldBe("Board B");
        history.Items[2].SyncedAt.ShouldBe(olderSync);
        history.Items[2].LeaderboardTitle.ShouldBe("Board A");
    }

    [Fact]
    public async Task GetMembers_IncludesLatestRaMetricsAndTrend_WhenTwoSnapshots()
    {
        // Arrange
        var olderSync = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var newerSync = new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var shrimp = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            db.MemberRaRankSnapshots.AddRange(
                new MemberRaRankSnapshot
                {
                    MemberId = shrimp.Id,
                    Rank = 120_000,
                    TotalRanked = 163_826,
                    TotalPoints = 500,
                    SyncedAt = olderSync
                },
                new MemberRaRankSnapshot
                {
                    MemberId = shrimp.Id,
                    Rank = 117_215,
                    TotalRanked = 163_826,
                    TotalPoints = 534,
                    TotalSoftcorePoints = 14,
                    SyncedAt = newerSync
                });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var members = await client.GetFromJsonAsync<List<MemberDto>>("/api/members", JsonOptions);

        // Assert
        members.ShouldNotBeNull();
        var shrimpRow = members.First(m => m.RaUsername == "ShrimpPoboy");
        shrimpRow.RaRank.ShouldBe(117_215);
        shrimpRow.RaTotalPoints.ShouldBe(534);
        shrimpRow.RaTotalSoftcorePoints.ShouldBe(14);
        shrimpRow.RaMetricsSyncedAt.ShouldBe(newerSync);
        shrimpRow.RaRankDelta.ShouldBe(120_000 - 117_215);
        shrimpRow.RaPointsDelta.ShouldBe(34);
    }

    [Fact]
    public async Task GetMembers_LastActiveAt_UsesRaSyncedPlayAndUnlocks()
    {
        // Arrange
        var playAt = new DateTimeOffset(2026, 2, 10, 12, 0, 0, TimeSpan.Zero);
        var unlockAt = new DateTimeOffset(2026, 2, 15, 8, 0, 0, TimeSpan.Zero);
        const int raGameId = 38130;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var shrimp = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            db.MemberRecentGamePlays.Add(new MemberRecentGamePlay
            {
                MemberId = shrimp.Id,
                RaGameId = raGameId,
                Title = "Pinball",
                ConsoleId = 1,
                LastPlayedAt = playAt,
                SyncedAt = playAt
            });
            db.RaAchievements.Add(new RaAchievement
            {
                RaAchievementId = 9010,
                RaGameId = raGameId,
                Title = "Play",
                Points = 1,
                TrueRatio = 1,
                DisplayOrder = 1
            });
            db.MemberRaAchievements.Add(new MemberRaAchievement
            {
                MemberId = shrimp.Id,
                RaAchievementId = 9010,
                DateEarned = unlockAt
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var members = await client.GetFromJsonAsync<List<MemberDto>>("/api/members", JsonOptions);

        // Assert
        members.ShouldNotBeNull();
        var shrimpRow = members.First(m => m.RaUsername == "ShrimpPoboy");
        shrimpRow.LastActiveAt.ShouldBe(unlockAt);
    }

    [Fact]
    public async Task GetMembers_IncludesPresence_WhenMemberRankSyncWrotePresence()
    {
        // Arrange
        var syncedAt = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var shrimp = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            shrimp.RaStatus = "Playing Game";
            shrimp.RaPresenceRaGameId = 38130;
            shrimp.RaPresenceGameTitle = "Pinball";
            shrimp.RaPresenceSyncedAt = syncedAt;
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var members = await client.GetFromJsonAsync<List<MemberDto>>("/api/members", JsonOptions);

        // Assert
        members.ShouldNotBeNull();
        var shrimpRow = members.First(m => m.RaUsername == "ShrimpPoboy");
        shrimpRow.RaStatus.ShouldBe("Playing Game");
        shrimpRow.RaPresenceRaGameId.ShouldBe(38130);
        shrimpRow.RaPresenceGameTitle.ShouldBe("Pinball");
        shrimpRow.RaPresenceIsTracked.ShouldBe(true);
        shrimpRow.RaPresenceSyncedAt.ShouldBe(syncedAt);
    }

    [Fact]
    public async Task GetMembersSummary_ReturnsMemberCountAndAchievementStats()
    {
        // Arrange
        const int raGameId = 38130;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            member.RaStatus = "Online";
            var game = await db.Games.SingleAsync(g => g.RaGameId == raGameId);
            var board = new Leaderboard
            {
                RaLeaderboardId = 3813098,
                GameId = game.Id,
                Title = "Summary Board",
                Format = "VALUE",
                RankAsc = false
            };
            db.Leaderboards.Add(board);
            await db.SaveChangesAsync();
            db.LeaderboardEntries.Add(new LeaderboardEntry
            {
                LeaderboardId = board.Id,
                MemberId = member.Id,
                Score = 100,
                FormattedScore = "100",
                FriendRank = 1,
                SyncedAt = DateTimeOffset.UtcNow
            });
            db.RaAchievements.Add(new RaAchievement
            {
                RaAchievementId = 9020,
                RaGameId = raGameId,
                Title = "Fresh",
                Points = 1,
                TrueRatio = 1,
                DisplayOrder = 1
            });
            db.MemberRaAchievements.Add(new MemberRaAchievement
            {
                MemberId = member.Id,
                RaAchievementId = 9020,
                DateEarned = DateTimeOffset.UtcNow.AddDays(-1)
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var summary = await client.GetFromJsonAsync<MembersSummaryDto>("/api/members/summary", JsonOptions);

        // Assert
        summary.ShouldNotBeNull();
        summary.MemberCount.ShouldBeGreaterThanOrEqualTo(2);
        summary.UnlocksLast7Days.ShouldBeGreaterThanOrEqualTo(1);
        summary.PlayingNowCount.ShouldBeGreaterThanOrEqualTo(1);
        summary.ChampionshipLeaderRaUsername.ShouldNotBeNullOrWhiteSpace();
    }
}
