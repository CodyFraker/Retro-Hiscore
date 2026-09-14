using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
    public async Task Sync_PersistsScores_AndLeavesMissingMembersEmpty()
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

        // Act
        var run = await sync.SyncAsync(SyncTrigger.Manual);
        var client = _factory.CreateAuthenticatedClient();
        var payload = await client.GetFromJsonAsync<GameLeaderboardsResponse>("/api/games/38130/leaderboards", JsonOptions);

        // Assert
        run.Status.ShouldBe(SyncRunStatus.Succeeded);
        payload.ShouldNotBeNull();
        payload.Leaderboards.Count.ShouldBe(1);
        payload.Leaderboards[0].GlobalEntryCount.ShouldBe(10_247);

        var shrimp = payload.Leaderboards[0].Standings.Single(s => s.RaUsername == "ShrimpPoboy");
        shrimp.Score.ShouldBe(352750);
        shrimp.FriendRank.ShouldBe(1);

        var missing = payload.Leaderboards[0].Standings.Where(s => s.RaUsername != "ShrimpPoboy").ToList();
        missing.ShouldAllBe(s => s.Score == null);
        missing.ShouldAllBe(s => s.FriendRank == null);
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

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.PostAsync("/api/sync", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }

    private sealed record MemberResponse(Guid Id, string RaUsername, string? RaUlid, string DisplayName);
}
