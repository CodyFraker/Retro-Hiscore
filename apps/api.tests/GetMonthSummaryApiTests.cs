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
public class GetMonthSummaryApiTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public GetMonthSummaryApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetMonthSummary_ReturnsSeptemberTotals_WhenActivityExistsInUtcMonth()
    {
        // Arrange
        var septemberUnlock = new DateTimeOffset(2026, 9, 15, 12, 0, 0, TimeSpan.Zero);
        var septemberPlay = new DateTimeOffset(2026, 9, 10, 8, 0, 0, TimeSpan.Zero);
        var augustSync = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);
        var septemberSync = new DateTimeOffset(2026, 9, 20, 12, 0, 0, TimeSpan.Zero);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var shrimp = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            var billy = await db.Members.SingleAsync(m => m.RaUsername == "beefboybilly");
            var game = await db.Games.SingleAsync(g => g.RaGameId == 38130);

            db.RaAchievements.Add(new RaAchievement
            {
                RaAchievementId = 9001,
                RaGameId = 38130,
                Title = "September unlock",
                Points = 10,
                TrueRatio = 25,
                DisplayOrder = 1,
                BadgeData = ApiFactory.SamplePngBytes,
                BadgeContentType = "image/png"
            });

            db.MemberRaAchievements.Add(new MemberRaAchievement
            {
                MemberId = shrimp.Id,
                RaAchievementId = 9001,
                DateEarned = septemberUnlock,
                DateEarnedHardcore = septemberUnlock,
                FirstDetectedAt = septemberUnlock
            });

            db.MemberRecentGamePlays.Add(new MemberRecentGamePlay
            {
                MemberId = billy.Id,
                RaGameId = 2291,
                Title = "Untracked play",
                ConsoleId = 1,
                LastPlayedAt = septemberPlay,
                NumAchieved = 0,
                NumPossibleAchievements = 10,
                SyncedAt = septemberPlay
            });

            var board = new Leaderboard
            {
                RaLeaderboardId = 3813099,
                GameId = game.Id,
                Title = "September board",
                RankAsc = false
            };
            db.Leaderboards.Add(board);
            await db.SaveChangesAsync();

            db.LeaderboardEntrySnapshots.AddRange(
                new LeaderboardEntrySnapshot
                {
                    LeaderboardId = board.Id,
                    MemberId = shrimp.Id,
                    Score = 50,
                    FormattedScore = "50",
                    FriendRank = 2,
                    SyncedAt = augustSync
                },
                new LeaderboardEntrySnapshot
                {
                    LeaderboardId = board.Id,
                    MemberId = shrimp.Id,
                    Score = 100,
                    FormattedScore = "100",
                    FriendRank = 1,
                    SyncedAt = septemberSync
                },
                new LeaderboardEntrySnapshot
                {
                    LeaderboardId = board.Id,
                    MemberId = billy.Id,
                    Score = 40,
                    FormattedScore = "40",
                    FriendRank = 2,
                    SyncedAt = augustSync
                },
                new LeaderboardEntrySnapshot
                {
                    LeaderboardId = board.Id,
                    MemberId = billy.Id,
                    Score = 60,
                    FormattedScore = "60",
                    FriendRank = 2,
                    SyncedAt = septemberSync
                });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var summary = await client.GetFromJsonAsync<MonthSummaryResponse>(
            "/api/dashboard/month-summary?year=2026&month=9",
            JsonOptions);

        // Assert
        summary.ShouldNotBeNull();
        summary.Year.ShouldBe(2026);
        summary.Month.ShouldBe(9);
        summary.GroupTotals.AchievementsUnlocked.ShouldBe(1);
        summary.GroupTotals.HardcoreUnlocks.ShouldBe(1);
        summary.GroupTotals.HardcorePoints.ShouldBe(10);
        summary.GroupTotals.TruePoints.ShouldBe(25);
        summary.GroupTotals.BoardsTaken.ShouldBe(1);
        summary.GroupTotals.ScoresImproved.ShouldBe(2);
        summary.GroupTotals.GamesPlayed.ShouldBe(2);

        var shrimpRow = summary.MemberRows.Single(r => r.RaUsername == "ShrimpPoboy");
        shrimpRow.AchievementsUnlocked.ShouldBe(1);
        shrimpRow.BoardsTaken.ShouldBe(1);
        shrimpRow.ScoresImproved.ShouldBe(1);

        var billyRow = summary.MemberRows.Single(r => r.RaUsername == "beefboybilly");
        billyRow.GamesPlayed.ShouldBe(1);
        billyRow.ScoresImproved.ShouldBe(1);

        summary.AvailableMonths.ShouldContain(m => m.Year == 2026 && m.Month == 9);
    }
}
