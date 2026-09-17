using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class MemberRecentGamesSyncMergeTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public MemberRecentGamesSyncMergeTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SyncMemberAsync_RetainsTrackedPlay_WhenAbsentFromRaResponse()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var member = await db.Members.FirstAsync(m => m.RaUsername != null);
        var game = await db.Games.FirstAsync();
        var syncedAt = DateTimeOffset.UtcNow;
        var lastPlayed = syncedAt.AddDays(-3);
        db.MemberRecentGamePlays.Add(new MemberRecentGamePlay
        {
            MemberId = member.Id,
            RaGameId = game.RaGameId,
            Title = game.Title,
            ConsoleId = 1,
            LastPlayedAt = lastPlayed,
            NumAchieved = 5,
            NumPossibleAchievements = 20,
            SyncedAt = syncedAt.AddHours(-1)
        });
        await db.SaveChangesAsync();

        _factory.RaApiClient
            .GetUserRecentlyPlayedGamesAsync(
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaUserRecentlyPlayedGameDto>>([]));

        var recentGamesSync = scope.ServiceProvider.GetRequiredService<IMemberRecentGamesSyncService>();

        // Act
        await recentGamesSync.SyncMemberAsync(member, syncedAt);

        // Assert
        var row = await db.MemberRecentGamePlays
            .SingleAsync(p => p.MemberId == member.Id && p.RaGameId == game.RaGameId);
        row.LastPlayedAt.ShouldBe(lastPlayed);
    }

    [Fact]
    public async Task SyncMemberAsync_RemovesUntrackedPlay_WhenAbsentFromRaResponse()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var member = await db.Members.FirstAsync(m => m.RaUsername != null);
        var syncedAt = DateTimeOffset.UtcNow;
        const int untrackedRaGameId = 888888;
        db.MemberRecentGamePlays.Add(new MemberRecentGamePlay
        {
            MemberId = member.Id,
            RaGameId = untrackedRaGameId,
            Title = "Untracked",
            ConsoleId = 1,
            LastPlayedAt = syncedAt.AddHours(-1),
            NumAchieved = 0,
            NumPossibleAchievements = 0,
            SyncedAt = syncedAt.AddHours(-2)
        });
        await db.SaveChangesAsync();

        _factory.RaApiClient
            .GetUserRecentlyPlayedGamesAsync(
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RaUserRecentlyPlayedGameDto>>([]));

        var recentGamesSync = scope.ServiceProvider.GetRequiredService<IMemberRecentGamesSyncService>();

        // Act
        await recentGamesSync.SyncMemberAsync(member, syncedAt);

        // Assert
        var exists = await db.MemberRecentGamePlays
            .AnyAsync(p => p.MemberId == member.Id && p.RaGameId == untrackedRaGameId);
        exists.ShouldBeFalse();
    }
}
