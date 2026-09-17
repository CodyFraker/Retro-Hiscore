using System.Net;
using System.Net.Http.Json;
using NSubstitute;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.GameOfTheWeek;
using RetroHiscore.Api.Features.Ra;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class PostAdminGameOfTheWeekClosePollEndpointTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public PostAdminGameOfTheWeekClosePollEndpointTests(ApiFactory factory)
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
    public async Task PostAdminClosePoll_ClosesOpenPollAndSetsWinner()
    {
        // Arrange
        await CreateOpenPollAsync([81001, 81002]);
        var voter = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);
        await voter.PutAsJsonAsync(
            "/api/game-of-the-week/current/vote",
            new GameOfTheWeekRaGameIdRequest(81002));

        var admin = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await admin.PostAsync("/api/admin/game-of-the-week/polls/current/close", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var poll = await response.Content.ReadFromJsonAsync<GameOfTheWeekCurrentPollDto>();
        poll.ShouldNotBeNull();
        poll!.ClosedAt.ShouldNotBeNull();
        poll.WinnerRaGameId.ShouldBe(81002);
        poll.Phase.ShouldBe(GameOfTheWeekPhase.Closed);
        poll.TrackingStatus.ShouldBe(GameOfTheWeekTrackingStatus.Pending);
    }

    [Fact]
    public async Task PostAdminClosePoll_WhenNoPoll_ReturnsNotFound()
    {
        // Arrange
        var admin = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await admin.PostAsync("/api/admin/game-of-the-week/polls/current/close", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostAdminClosePoll_WhenScheduled_ReturnsConflict()
    {
        // Arrange
        StubRaGame(82001, "Future A");
        StubRaGame(82002, "Future B");
        var admin = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);
        var create = await admin.PostAsJsonAsync(
            "/api/admin/game-of-the-week/polls",
            new PostGameOfTheWeekPollRequest(
                DateTimeOffset.UtcNow.AddDays(1),
                DateTimeOffset.UtcNow.AddDays(7),
                [82001, 82002]));
        create.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Act
        var response = await admin.PostAsync("/api/admin/game-of-the-week/polls/current/close", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PostAdminClosePoll_WhenNotAdmin_ReturnsForbidden()
    {
        // Arrange
        await CreateOpenPollAsync([83001, 83002]);
        var member = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var response = await member.PostAsync("/api/admin/game-of-the-week/polls/current/close", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostAdminClosePoll_WhenAlreadyClosed_ReturnsConflict()
    {
        // Arrange
        await CreateOpenPollAsync([84001, 84002]);
        var admin = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);
        var first = await admin.PostAsync("/api/admin/game-of-the-week/polls/current/close", null);
        first.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act
        var second = await admin.PostAsync("/api/admin/game-of-the-week/polls/current/close", null);

        // Assert
        second.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    private void StubRaGame(int raGameId, string title)
    {
        _factory.RaApiClient
            .GetGameAsync(raGameId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new RaGameDto
            {
                Title = title,
                ConsoleId = 1,
                ConsoleName = "NES"
            });
    }

    private async Task CreateOpenPollAsync(int[] raGameIds)
    {
        foreach (var id in raGameIds)
        {
            StubRaGame(id, $"Game {id}");
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
}
