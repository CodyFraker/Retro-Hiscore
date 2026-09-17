using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Admin;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class PostAdminGameTrackQueueTrackEndpointTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public PostAdminGameTrackQueueTrackEndpointTests(ApiFactory factory)
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
    public async Task PostAdminGameTrackQueueTrack_CreatesGameAndCompletesQueue()
    {
        // Arrange
        const int raGameId = 88001;
        SetupRaMocks(raGameId);
        Guid queueId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var item = new GameTrackQueue
            {
                RaGameId = raGameId,
                Status = GameTrackQueueStatus.Pending,
                Title = "Track From Queue",
                Source = GameTrackQueueSource.MemberRequest,
                RequestCount = 1,
                EnqueuedAt = DateTimeOffset.UtcNow
            };
            db.GameTrackQueues.Add(item);
            await db.SaveChangesAsync();
            queueId = item.Id;
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var response = await client.PostAsync($"/api/admin/game-track-queue/{queueId}/track", null);
        var game = await response.Content.ReadFromJsonAsync<AdminGameDto>(JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        game.ShouldNotBeNull();
        game!.RaGameId.ShouldBe(raGameId);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var queue = await verifyDb.GameTrackQueues.SingleAsync(q => q.Id == queueId);
        queue.Status.ShouldBe(GameTrackQueueStatus.Completed);
        queue.ResolvedAt.ShouldNotBeNull();
    }

    private void SetupRaMocks(int raGameId)
    {
        _factory.RaApiClient
            .GetGameAsync(raGameId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new RaGameDto
            {
                Title = "Queue Track Game",
                ConsoleId = 3,
                ConsoleName = "SNES"
            }));

        _factory.RaApiClient
            .GetGameLeaderboardsAsync(raGameId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaGameLeaderboardDto>>([]));

        _factory.RaApiClient
            .GetConsoleIdsAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaConsoleIdDto>>([]));
    }
}
