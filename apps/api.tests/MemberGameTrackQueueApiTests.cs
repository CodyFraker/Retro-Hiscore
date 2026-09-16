using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Games;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class MemberGameTrackQueueApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public MemberGameTrackQueueApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetGameTrackQueue_ReturnsPendingItemsForMembers()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.GameTrackQueues.Add(new GameTrackQueue
            {
                RaGameId = 424242,
                Status = GameTrackQueueStatus.Pending,
                Title = "Member Visible",
                ConsoleName = "SNES",
                EnqueuedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.SecondAllowedDiscordUserId);

        // Act
        var items = await client.GetFromJsonAsync<List<GameTrackQueueItemDto>>("/api/games/track-queue");

        // Assert
        items.ShouldNotBeNull();
        items.Count.ShouldBe(1);
        items[0].RaGameId.ShouldBe(424242);
        items[0].Status.ShouldBe("Pending");
    }
}
