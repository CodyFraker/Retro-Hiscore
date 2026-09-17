using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.GameOfTheWeek;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Ra;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class PostGameTrackRequestEndpointTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public PostGameTrackRequestEndpointTests(ApiFactory factory)
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
    public async Task PostGameTrackRequest_CreatesPendingQueueItem()
    {
        // Arrange
        const int raGameId = 55555;
        SetupRaGameMock(raGameId);
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/games/track-requests",
            new GameOfTheWeekRaGameIdRequest(raGameId));
        var body = await response.Content.ReadFromJsonAsync<GameTrackRequestSubmitResponse>(JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        body.ShouldNotBeNull();
        body!.Code.ShouldBe("created");
        body.RaGameId.ShouldBe(raGameId);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var queue = await db.GameTrackQueues.SingleAsync(q => q.RaGameId == raGameId);
        queue.Status.ShouldBe(GameTrackQueueStatus.Pending);
        queue.Source.ShouldBe(GameTrackQueueSource.MemberRequest);
        queue.RequestCount.ShouldBe(1);
    }

    [Fact]
    public async Task PostGameTrackRequest_Returns409_WhenAlreadyTracked()
    {
        // Arrange
        const int raGameId = 38130;
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/games/track-requests",
            new GameOfTheWeekRaGameIdRequest(raGameId));
        var body = await response.Content.ReadFromJsonAsync<GameTrackRequestSubmitResponse>(JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        body.ShouldNotBeNull();
        body!.Code.ShouldBe("alreadyTracked");
    }

    [Fact]
    public async Task PostGameTrackRequest_IsIdempotent_WhenSameMemberPendingAgain()
    {
        // Arrange
        const int raGameId = 66666;
        SetupRaGameMock(raGameId);
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var first = await client.PostAsJsonAsync(
            "/api/games/track-requests",
            new GameOfTheWeekRaGameIdRequest(raGameId));
        var second = await client.PostAsJsonAsync(
            "/api/games/track-requests",
            new GameOfTheWeekRaGameIdRequest(raGameId));

        // Assert
        first.StatusCode.ShouldBe(HttpStatusCode.Created);
        second.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.GameTrackQueueRequests.CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task PostGameTrackRequest_Returns429_AfterFiveInRollingWindow()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);
        SetupRaGameMock(10001);
        SetupRaGameMock(10002);
        SetupRaGameMock(10003);
        SetupRaGameMock(10004);
        SetupRaGameMock(10005);
        SetupRaGameMock(10006);

        // Act
        for (var id = 10001; id <= 10005; id++)
        {
            var ok = await client.PostAsJsonAsync(
                "/api/games/track-requests",
                new GameOfTheWeekRaGameIdRequest(id));
            ok.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        var sixth = await client.PostAsJsonAsync(
            "/api/games/track-requests",
            new GameOfTheWeekRaGameIdRequest(10006));
        var body = await sixth.Content.ReadFromJsonAsync<GameTrackRequestSubmitResponse>(JsonOptions);

        // Assert
        sixth.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        body.ShouldNotBeNull();
        body!.Code.ShouldBe("quotaExceeded");
        body.NextSlotAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task PostGameTrackRequest_ReopensRejectedQueueItem()
    {
        // Arrange
        const int raGameId = 77701;
        SetupRaGameMock(raGameId);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.GameTrackQueues.Add(new GameTrackQueue
            {
                RaGameId = raGameId,
                Status = GameTrackQueueStatus.Rejected,
                Title = "Old",
                EnqueuedAt = DateTimeOffset.UtcNow.AddDays(-1),
                ResolvedAt = DateTimeOffset.UtcNow.AddDays(-1)
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/games/track-requests",
            new GameOfTheWeekRaGameIdRequest(raGameId));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var queue = await verifyDb.GameTrackQueues.SingleAsync(q => q.RaGameId == raGameId);
        queue.Status.ShouldBe(GameTrackQueueStatus.Pending);
        queue.Source.ShouldBe(GameTrackQueueSource.MemberRequest);
    }

    private void SetupRaGameMock(int raGameId)
    {
        _factory.RaApiClient
            .GetGameAsync(raGameId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new RaGameDto
            {
                Title = $"Game {raGameId}",
                ConsoleName = "SNES"
            }));
    }
}
