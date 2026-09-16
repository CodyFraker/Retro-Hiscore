using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.GameOfTheWeek;
using RetroHiscore.Api.Features.Ra;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class GameOfTheWeekApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public GameOfTheWeekApiTests(ApiFactory factory)
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
    public async Task PostAdminPoll_WithLessThanTwoGames_ReturnsValidationProblem()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);
        var body = new PostGameOfTheWeekPollRequest(
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddDays(7),
            [100]);

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/game-of-the-week/polls", body);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostAdminPoll_CreatesPollWithBallot()
    {
        // Arrange
        StubRaGame(20001, "Seed One");
        StubRaGame(20002, "Seed Two");
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);
        var starts = DateTimeOffset.UtcNow.AddMinutes(-5);
        var ends = DateTimeOffset.UtcNow.AddDays(3);
        var body = new PostGameOfTheWeekPollRequest(starts, ends, [20001, 20002]);

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/game-of-the-week/polls", body);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var poll = await response.Content.ReadFromJsonAsync<GameOfTheWeekCurrentPollDto>();
        poll.ShouldNotBeNull();
        poll!.Ballot.Count.ShouldBe(2);
        poll.Phase.ShouldBe(GameOfTheWeekPhase.Open);
        poll.BallotSlotsRemaining.ShouldBe(3);
    }

    [Fact]
    public async Task PostBallot_AddsThirdGameWhileOpen()
    {
        // Arrange
        await CreateOpenPollAsync([30001, 30002]);
        StubRaGame(30003, "Member Pick");
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/game-of-the-week/current/ballot",
            new GameOfTheWeekRaGameIdRequest(30003));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var poll = await response.Content.ReadFromJsonAsync<GameOfTheWeekCurrentPollDto>();
        poll!.Ballot.Count.ShouldBe(3);
        poll.BallotSlotsRemaining.ShouldBe(2);
    }

    [Fact]
    public async Task PutVote_CanChangeVoteWithoutMatchingBallotNominee()
    {
        // Arrange
        await CreateOpenPollAsync([40001, 40002]);
        StubRaGame(40003, "Added By Member");
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);
        await client.PostAsJsonAsync(
            "/api/game-of-the-week/current/ballot",
            new GameOfTheWeekRaGameIdRequest(40003));

        // Act
        var firstVote = await client.PutAsJsonAsync(
            "/api/game-of-the-week/current/vote",
            new GameOfTheWeekRaGameIdRequest(40001));
        var changedVote = await client.PutAsJsonAsync(
            "/api/game-of-the-week/current/vote",
            new GameOfTheWeekRaGameIdRequest(40002));

        // Assert
        firstVote.StatusCode.ShouldBe(HttpStatusCode.OK);
        changedVote.StatusCode.ShouldBe(HttpStatusCode.OK);
        var poll = await changedVote.Content.ReadFromJsonAsync<GameOfTheWeekCurrentPollDto>();
        poll!.MyVoteRaGameId.ShouldBe(40002);
        poll.Ballot.Single(b => b.RaGameId == 40001).VoteCount.ShouldBe(0);
        poll.Ballot.Single(b => b.RaGameId == 40002).VoteCount.ShouldBe(1);
    }

    [Fact]
    public async Task CloseJob_SetsPendingTrackingForUntrackedWinner()
    {
        // Arrange
        await CreateOpenPollAsync([50001, 50002]);
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);
        await client.PutAsJsonAsync(
            "/api/game-of-the-week/current/vote",
            new GameOfTheWeekRaGameIdRequest(50002));

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var poll = await db.GameOfTheWeekPolls.SingleAsync();
            poll.EndsAt = DateTimeOffset.UtcNow.AddMinutes(-1);
            await db.SaveChangesAsync();
        }

        // Act
        using (var scope = _factory.Services.CreateScope())
        {
            var close = scope.ServiceProvider.GetRequiredService<IGameOfTheWeekCloseService>();
            await close.CloseDuePollsAsync(CancellationToken.None);
        }

        // Assert
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var poll = await db.GameOfTheWeekPolls.SingleAsync();
            poll.ClosedAt.ShouldNotBeNull();
            poll.WinnerRaGameId.ShouldBe(50002);
            poll.TrackingStatus.ShouldBe(GameOfTheWeekTrackingStatus.Pending);
            (await db.Games.AnyAsync(g => g.RaGameId == 50002)).ShouldBeFalse();
        }
    }

    [Fact]
    public async Task GetHistory_ExcludesPendingPollAndOrdersByClosedAt()
    {
        // Arrange
        StubRaGame(70001, "Older Winner");
        StubRaGame(70002, "Older Runner");
        StubRaGame(71001, "Newer Winner");
        StubRaGame(71002, "Newer Runner");
        await CreateCompletedPollAsync(70001, 70002, winnerRaGameId: 70001, closedAt: DateTimeOffset.UtcNow.AddDays(-14));
        await CreateClosedPendingPollAsync(71001, 71002, winnerRaGameId: 71001);

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var response = await client.GetAsync("/api/game-of-the-week/history?limit=10");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var history = await response.Content.ReadFromJsonAsync<GameOfTheWeekHistoryResponse>();
        history.ShouldNotBeNull();
        history!.Total.ShouldBe(1);
        history.Items.Count.ShouldBe(1);
        history.Items[0].WinnerRaGameId.ShouldBe(70001);
        history.Items[0].WinnerTitle.ShouldBe("Older Winner");
    }

    [Fact]
    public async Task WinnerTrackingJob_CreatesGameWhenRaSucceeds()
    {
        // Arrange
        StubRaGame(60001, "Winner Untracked");
        _factory.RaApiClient.GetGameLeaderboardsAsync(60001, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<RaGameLeaderboardDto>());
        await CreateClosedPendingPollAsync(60001, 60002, winnerRaGameId: 60001);

        // Act
        using (var scope = _factory.Services.CreateScope())
        {
            var tracking = scope.ServiceProvider.GetRequiredService<IGameOfTheWeekWinnerTrackingService>();
            await tracking.ProcessPendingWinnersAsync(CancellationToken.None);
        }

        // Assert
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var poll = await db.GameOfTheWeekPolls.SingleAsync();
            poll.TrackingStatus.ShouldBe(GameOfTheWeekTrackingStatus.Completed);
            (await db.Games.AnyAsync(g => g.RaGameId == 60001)).ShouldBeTrue();
        }
    }

    private void StubRaGame(int raGameId, string title)
    {
        _factory.RaApiClient.GetGameAsync(raGameId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new RaGameDto
            {
                Title = title,
                ConsoleId = 1,
                ConsoleName = "NES"
            });
    }

    private async Task CreateOpenPollAsync(int[] raGameIds, bool stubGames = true)
    {
        if (stubGames)
        {
            foreach (var id in raGameIds)
            {
                StubRaGame(id, $"Game {id}");
            }
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);
        var response = await client.PostAsJsonAsync(
            "/api/admin/game-of-the-week/polls",
            new PostGameOfTheWeekPollRequest(
                DateTimeOffset.UtcNow.AddMinutes(-1),
                DateTimeOffset.UtcNow.AddDays(2),
                raGameIds));
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    private async Task CreateCompletedPollAsync(
        int seedA,
        int seedB,
        int winnerRaGameId,
        DateTimeOffset closedAt)
    {
        await CreateOpenPollAsync([seedA, seedB], stubGames: false);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var poll = await db.GameOfTheWeekPolls.OrderByDescending(p => p.CreatedAt).FirstAsync();
        poll.ClosedAt = closedAt;
        poll.WinnerRaGameId = winnerRaGameId;
        poll.TrackingStatus = GameOfTheWeekTrackingStatus.Completed;
        poll.EndsAt = closedAt.AddMinutes(-5);
        await db.SaveChangesAsync();
    }

    private async Task CreateClosedPendingPollAsync(int seedA, int seedB, int winnerRaGameId)
    {
        StubRaGame(seedA, $"Game {seedA}");
        StubRaGame(seedB, $"Game {seedB}");
        await CreateOpenPollAsync([seedA, seedB], stubGames: false);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var poll = await db.GameOfTheWeekPolls.OrderByDescending(p => p.CreatedAt).FirstAsync();
        poll.ClosedAt = DateTimeOffset.UtcNow;
        poll.WinnerRaGameId = winnerRaGameId;
        poll.TrackingStatus = GameOfTheWeekTrackingStatus.Pending;
        poll.EndsAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        await db.SaveChangesAsync();
    }
}
