using System.Net;
using System.Net.Http.Json;
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
public class GameTrackQueueApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public GameTrackQueueApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAdminGameTrackQueue_ReturnsPendingItems()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.GameTrackQueues.Add(new GameTrackQueue
            {
                RaGameId = 77777,
                Status = GameTrackQueueStatus.Pending,
                Title = "Queued Title",
                ConsoleName = "NES",
                EnqueuedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);

        // Act
        var items = await client.GetFromJsonAsync<List<AdminGameTrackQueueItemDto>>("/api/admin/game-track-queue");

        // Assert
        items.ShouldNotBeNull();
        items.Count.ShouldBe(1);
        items[0].RaGameId.ShouldBe(77777);
        items[0].Status.ShouldBe(GameTrackQueueStatus.Pending);
    }

    [Fact]
    public async Task PostReject_BlocksReEnqueueForSameRaGameId()
    {
        // Arrange
        Guid queueId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var item = new GameTrackQueue
            {
                RaGameId = 88888,
                Status = GameTrackQueueStatus.Pending,
                Title = "Reject Me",
                EnqueuedAt = DateTimeOffset.UtcNow
            };
            db.GameTrackQueues.Add(item);
            await db.SaveChangesAsync();
            queueId = item.Id;
        }

        var client = _factory.CreateAuthenticatedClient(AuthTestHelper.AdminDiscordUserId);
        var rejectResponse = await client.PostAsync($"/api/admin/game-track-queue/{queueId}/reject", null);
        rejectResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var queueService = scope.ServiceProvider.GetRequiredService<IGameTrackQueueService>();
            var shrimp = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
            db.MemberRecentGamePlays.Add(new MemberRecentGamePlay
            {
                MemberId = shrimp.Id,
                RaGameId = 88888,
                Title = "Reject Me",
                ConsoleId = 1,
                LastPlayedAt = DateTimeOffset.UtcNow,
                NumAchieved = 5,
                NumPossibleAchievements = 10,
                SyncedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();

            // Act
            await queueService.EnqueueEligibleAsync(DateTimeOffset.UtcNow);

            // Assert
            var pending = await db.GameTrackQueues
                .Where(q => q.RaGameId == 88888 && q.Status == GameTrackQueueStatus.Pending)
                .CountAsync();
            pending.ShouldBe(0);
        }
    }

}
