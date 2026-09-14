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
public class LeaderboardSyncDispatcherTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public LeaderboardSyncDispatcherTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task DispatchDueGamesAsync_EnqueuesOnlyDueHotGames()
    {
        // Arrange
        var recording = new RecordingLeaderboardSyncJobEnqueuer();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hotGame = await db.Games.FirstAsync();
        var coldGame = new Game
        {
            RaGameId = 99999,
            Title = "Cold Game",
            LeaderboardScoresSyncedAt = DateTimeOffset.UtcNow
        };
        db.Games.Add(coldGame);
        var member = await db.Members.FirstAsync(m => m.RaUsername != null);
        db.MemberRecentGamePlays.Add(new MemberRecentGamePlay
        {
            MemberId = member.Id,
            RaGameId = hotGame.RaGameId,
            Title = hotGame.Title,
            ConsoleId = 1,
            LastPlayedAt = DateTimeOffset.UtcNow,
            SyncedAt = DateTimeOffset.UtcNow
        });
        hotGame.LeaderboardScoresSyncedAt = DateTimeOffset.UtcNow.AddHours(-1);
        foreach (var other in await db.Games.Where(g => g.Id != hotGame.Id && g.Id != coldGame.Id).ToListAsync())
        {
            other.LeaderboardScoresSyncedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync();

        var syncSettings = new FixedSyncSettingsStore(new LeaderboardSyncPolicy(15, 1440, 168));
        var options = Microsoft.Extensions.Options.Options.Create(new SyncOptions());
        var dispatcher = new LeaderboardSyncDispatcher(
            db,
            recording,
            syncSettings,
            options,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<LeaderboardSyncDispatcher>.Instance);

        // Act
        var count = await dispatcher.DispatchDueGamesAsync(SyncTrigger.Scheduled, forceAll: false);

        // Assert
        count.ShouldBe(1);
        recording.FullGameEnqueues.Count.ShouldBe(1);
        recording.FullGameEnqueues[0].RaGameId.ShouldBe(hotGame.RaGameId);
    }
}
