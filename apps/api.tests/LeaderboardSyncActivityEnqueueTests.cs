using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Options;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class LeaderboardSyncActivityEnqueueTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public LeaderboardSyncActivityEnqueueTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task EnqueueForMemberChangesAsync_EnqueuesMemberGame_ForTrackedGameWithUpdate()
    {
        // Arrange
        var recording = new RecordingLeaderboardSyncJobEnqueuer();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var syncSettings = scope.ServiceProvider.GetRequiredService<ISyncSettingsStore>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<SyncOptions>>();
        var service = new LeaderboardSyncActivityEnqueueService(
            db,
            syncSettings,
            recording,
            options,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<LeaderboardSyncActivityEnqueueService>.Instance);

        var game = await db.Games.FirstAsync();
        var member = await db.Members.FirstAsync(m => m.RaUsername != null);
        member.RaApiKey = "key";
        await db.SaveChangesAsync();

        var now = DateTimeOffset.UtcNow;
        var changes = new[]
        {
            new MemberRecentGamePlayChange(member.Id, game.RaGameId, now, 2, 10, true)
        };

        // Act
        await service.EnqueueForMemberChangesAsync(changes, SyncTrigger.Scheduled);

        // Assert
        recording.MemberEnqueues.Count.ShouldBe(1);
        recording.MemberEnqueues[0].RaGameId.ShouldBe(game.RaGameId);
        recording.MemberEnqueues[0].MemberId.ShouldBe(member.Id);
    }

    [Fact]
    public async Task EnqueueForMemberChangesAsync_SkipsUntrackedGame()
    {
        // Arrange
        var recording = new RecordingLeaderboardSyncJobEnqueuer();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var syncSettings = scope.ServiceProvider.GetRequiredService<ISyncSettingsStore>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<SyncOptions>>();
        var service = new LeaderboardSyncActivityEnqueueService(
            db,
            syncSettings,
            recording,
            options,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<LeaderboardSyncActivityEnqueueService>.Instance);

        var member = await db.Members.FirstAsync(m => m.RaUsername != null);
        var now = DateTimeOffset.UtcNow;
        var changes = new[]
        {
            new MemberRecentGamePlayChange(member.Id, 999999, now, 2, 10, true)
        };

        // Act
        await service.EnqueueForMemberChangesAsync(changes, SyncTrigger.Scheduled);

        // Assert
        recording.MemberEnqueues.ShouldBeEmpty();
    }
}
