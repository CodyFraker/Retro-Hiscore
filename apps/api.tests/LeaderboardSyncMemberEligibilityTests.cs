using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

[Collection("Integration")]
public class LeaderboardSyncMemberEligibilityTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public LeaderboardSyncMemberEligibilityTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetEligibleMemberIdsAsync_ReturnsMemberInWindow_WithApiKey()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var game = await db.Games.FirstAsync();
        var member = await db.Members.FirstAsync(m => m.RaUsername != null);
        member.RaApiKey = "member-key";
        var policy = new LeaderboardSyncPolicy(15, 1440, 168);
        var now = DateTimeOffset.UtcNow;
        db.MemberRecentGamePlays.Add(new MemberRecentGamePlay
        {
            MemberId = member.Id,
            RaGameId = game.RaGameId,
            Title = game.Title,
            ConsoleId = 1,
            LastPlayedAt = now.AddHours(-1),
            NumAchieved = 1,
            NumPossibleAchievements = 10,
            SyncedAt = now
        });
        await db.SaveChangesAsync();

        // Act
        var ids = await LeaderboardSyncMemberEligibility.GetEligibleMemberIdsAsync(
            db,
            game.RaGameId,
            now,
            policy);

        // Assert
        ids.ShouldContain(member.Id);
    }

    [Fact]
    public async Task GetEligibleMemberIdsAsync_ExcludesMemberOutsideWindow()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var game = await db.Games.FirstAsync();
        var member = await db.Members.FirstAsync(m => m.RaUsername != null);
        member.RaApiKey = "member-key";
        var policy = new LeaderboardSyncPolicy(15, 1440, 168);
        var now = DateTimeOffset.UtcNow;
        db.MemberRecentGamePlays.Add(new MemberRecentGamePlay
        {
            MemberId = member.Id,
            RaGameId = game.RaGameId,
            Title = game.Title,
            ConsoleId = 1,
            LastPlayedAt = now.AddDays(-30),
            NumAchieved = 1,
            NumPossibleAchievements = 10,
            SyncedAt = now
        });
        await db.SaveChangesAsync();

        // Act
        var ids = await LeaderboardSyncMemberEligibility.GetEligibleMemberIdsAsync(
            db,
            game.RaGameId,
            now,
            policy);

        // Assert
        ids.ShouldNotContain(member.Id);
    }

    [Fact]
    public async Task GetEligibleMemberIdsAsync_ExcludesMemberWithoutApiKey()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var game = await db.Games.FirstAsync();
        var member = await db.Members.FirstAsync(m => m.RaUsername != null);
        member.RaApiKey = null;
        var policy = new LeaderboardSyncPolicy(15, 1440, 168);
        var now = DateTimeOffset.UtcNow;
        db.MemberRecentGamePlays.Add(new MemberRecentGamePlay
        {
            MemberId = member.Id,
            RaGameId = game.RaGameId,
            Title = game.Title,
            ConsoleId = 1,
            LastPlayedAt = now.AddHours(-1),
            NumAchieved = 1,
            NumPossibleAchievements = 10,
            SyncedAt = now
        });
        await db.SaveChangesAsync();

        // Act
        var ids = await LeaderboardSyncMemberEligibility.GetEligibleMemberIdsAsync(
            db,
            game.RaGameId,
            now,
            policy);

        // Assert
        ids.ShouldBeEmpty();
    }
}
