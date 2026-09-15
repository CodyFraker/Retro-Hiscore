using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class LeaderboardApiTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public LeaderboardApiTests(ApiFactory factory)
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
    public async Task GetMembers_ReturnsSeededPlayers()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var members = await client.GetFromJsonAsync<List<MemberResponse>>("/api/members", JsonOptions);

        // Assert
        members.ShouldNotBeNull();
        members.Select(m => m.RaUsername).ShouldBe(
            ["beefboybilly", "ShrimpPoboy", "xXScubXx"],
            ignoreOrder: true);
    }

    [Fact]
    public async Task GetGames_ReturnsSeededGames()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var games = await client.GetFromJsonAsync<List<GameDto>>("/api/games", JsonOptions);

        // Assert
        games.ShouldNotBeNull();
        games.Select(g => g.RaGameId).ShouldBe([38130, 2291, 789], ignoreOrder: true);
    }

    [Fact]
    public async Task Sync_PersistsScores_AndExcludesNonEngagedMembers()
    {
        // Arrange
        await _factory.SetMemberApiKeyAsync("ShrimpPoboy", "shrimp-test-key");
        _factory.RaApiClient
            .GetGameLeaderboardsAsync(Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var gameId = call.ArgAt<int>(0);
                return Task.FromResult<IReadOnlyList<RaGameLeaderboardDto>>(
                [
                    new RaGameLeaderboardDto
                    {
                        Id = gameId * 10L,
                        Title = "Main board",
                        Description = "Highest score",
                        Format = "VALUE",
                        RankAsc = false
                    }
                ]);
            });

        _factory.RaApiClient
            .GetUserGameLeaderboardsAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var gameId = call.ArgAt<int>(0);
                var identity = call.ArgAt<string>(1);
                var isShrimp = string.Equals(identity, "ShrimpPoboy", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(identity, "01TESTULID", StringComparison.OrdinalIgnoreCase);
                if (!isShrimp)
                {
                    return Task.FromResult<IReadOnlyList<RaUserGameLeaderboardDto>>([]);
                }

                return Task.FromResult<IReadOnlyList<RaUserGameLeaderboardDto>>(
                [
                    new RaUserGameLeaderboardDto
                    {
                        Id = gameId * 10L,
                        Title = "Main board",
                        RankAsc = false,
                        Format = "VALUE",
                        UserEntry = new RaUserEntryDto
                        {
                            User = "ShrimpPoboy",
                            Ulid = "01TESTULID",
                            Score = 352750,
                            FormattedScore = "352,750",
                            Rank = 259,
                            DateUpdated = DateTimeOffset.UtcNow
                        }
                    }
                ]);
            });

        _factory.RaApiClient
            .GetLeaderboardEntryCountAsync(Arg.Any<long>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(10_247);

        using var scope = _factory.Services.CreateScope();
        var sync = scope.ServiceProvider.GetRequiredService<ILeaderboardSyncService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var game = await db.Games.SingleAsync(g => g.RaGameId == 38130);

        // Act
        var run = await sync.SyncGameWithRunAsync(game, SyncTrigger.Manual);
        var client = _factory.CreateAuthenticatedClient();
        var payload = await client.GetFromJsonAsync<GameLeaderboardsResponse>("/api/games/38130/leaderboards", JsonOptions);

        // Assert
        run.Status.ShouldBe(SyncRunStatus.Succeeded);
        payload.ShouldNotBeNull();
        payload.Leaderboards.Count.ShouldBe(1);
        payload.Leaderboards[0].GlobalEntryCount.ShouldBe(10_247);

        payload.Members.Count.ShouldBe(1);
        payload.Members[0].RaUsername.ShouldBe("ShrimpPoboy");

        var shrimp = payload.Leaderboards[0].Standings.Single(s => s.RaUsername == "ShrimpPoboy");
        shrimp.Score.ShouldBe(352750);
        shrimp.FriendRank.ShouldBe(1);
        payload.Leaderboards[0].Standings.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetGameLeaderboards_ReturnsHotLeaderboardSyncStatus_WhenGroupPlayedRecently()
    {
        // Arrange
        const int raGameId = 38130;
        var recentPlay = DateTimeOffset.UtcNow.AddHours(-1);
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
                ConsoleName = "Genesis",
                LastPlayedAt = recentPlay,
                NumAchieved = 2,
                NumPossibleAchievements = 20,
                SyncedAt = recentPlay
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var payload = await client.GetFromJsonAsync<GameLeaderboardsResponse>(
            $"/api/games/{raGameId}/leaderboards",
            JsonOptions);

        // Assert
        payload.ShouldNotBeNull();
        payload!.LeaderboardSyncStatus.Tier.ShouldBe("Hot");
        payload.LeaderboardSyncStatus.LeaderboardSyncIntervalMinutes.ShouldBe(15);
        payload.LeaderboardSyncStatus.GroupLastPlayedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetGameLeaderboards_IncludesMemberEngagedViaAchievementsOnly()
    {
        // Arrange
        const int raGameId = 38130;
        const int achievementId = 8001;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var game = await db.Games.SingleAsync(g => g.RaGameId == raGameId);
            var member = await db.Members.SingleAsync(m => m.RaUsername == "beefboybilly");
            db.Leaderboards.Add(new Leaderboard
            {
                GameId = game.Id,
                RaLeaderboardId = 38130999,
                Title = "Test board",
                Format = "VALUE",
                RankAsc = false
            });
            db.RaAchievements.Add(new RaAchievement
            {
                RaAchievementId = achievementId,
                RaGameId = raGameId,
                Title = "Engaged only",
                Points = 5,
                TrueRatio = 10,
                DisplayOrder = 1
            });
            db.MemberRaAchievements.Add(new MemberRaAchievement
            {
                MemberId = member.Id,
                RaAchievementId = achievementId,
                DateEarned = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var payload = await client.GetFromJsonAsync<GameLeaderboardsResponse>(
            $"/api/games/{raGameId}/leaderboards",
            JsonOptions);

        // Assert
        payload.ShouldNotBeNull();
        payload!.Members.Select(m => m.RaUsername).ShouldBe(["beefboybilly"]);
        payload.Leaderboards.ShouldNotBeEmpty();
        foreach (var board in payload.Leaderboards)
        {
            board.Standings.Count.ShouldBe(1);
            board.Standings[0].RaUsername.ShouldBe("beefboybilly");
            board.Standings[0].Score.ShouldBeNull();
        }
    }

    [Fact]
    public async Task TriggerSync_Returns429_WhenCooldownActive()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.SyncRuns.Add(new SyncRun
            {
                Kind = SyncKind.LeaderboardScores,
                Trigger = SyncTrigger.Manual,
                Status = SyncRunStatus.Succeeded,
                StartedAt = DateTimeOffset.UtcNow,
                FinishedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PostAsync("/api/sync", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }

    private sealed record MemberResponse(Guid Id, string RaUsername, string? RaUlid, string DisplayName);
}
