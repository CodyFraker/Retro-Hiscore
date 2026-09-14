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
public class MemberRaGameProgressSyncTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public MemberRaGameProgressSyncTests(ApiFactory factory)
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
    public async Task SyncMemberGamesFromSummary_PersistsUnlockAndBadge_WithoutDuplicating()
    {
        // Arrange
        await _factory.SetMemberApiKeyAsync("ShrimpPoboy", "shrimp-key");
        const int gameId = 38130;
        const int achievementId = 9001;

        _factory.RaApiClient
            .GetGameInfoAndUserProgressAsync(
                gameId,
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<RaGameInfoAndUserProgressDto?>(new RaGameInfoAndUserProgressDto
            {
                Id = gameId,
                Achievements = new Dictionary<string, RaGameAchievementProgressDto>
                {
                    ["9001"] = new RaGameAchievementProgressDto
                    {
                        Id = achievementId,
                        Title = "Synced Unlock",
                        Description = "From RA",
                        Points = 5,
                        TrueRatio = 10,
                        BadgeName = "badge-9001",
                        DisplayOrder = 1,
                        DateEarned = "2024-06-01 10:00:00",
                        DateEarnedHardcore = "2024-06-01 10:00:00"
                    }
                }
            }));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var member = await db.Members.SingleAsync(m => m.RaUsername == "ShrimpPoboy");
        var sync = scope.ServiceProvider.GetRequiredService<IMemberRaGameProgressSyncService>();
        var syncedAt = DateTimeOffset.UtcNow;

        var summary = new RaUserSummaryDto
        {
            User = "ShrimpPoboy",
            LastGame = new RaUserSummaryGameDto { Id = gameId, Title = "Test Game", ConsoleId = 1 }
        };

        // Act
        await sync.SyncMemberGamesFromSummaryAsync(member, summary, syncedAt);
        await sync.SyncMemberGamesFromSummaryAsync(member, summary, syncedAt);

        // Assert
        var catalog = await db.RaAchievements.SingleAsync(a => a.RaAchievementId == achievementId);
        catalog.RaGameId.ShouldBe(gameId);
        catalog.Title.ShouldBe("Synced Unlock");
        catalog.BadgeData.ShouldBe(ApiFactory.SamplePngBytes);

        var unlocks = await db.MemberRaAchievements
            .Where(m => m.MemberId == member.Id && m.RaAchievementId == achievementId)
            .ToListAsync();
        unlocks.Count.ShouldBe(1);
        unlocks[0].DateEarned.ShouldNotBeNull();
        unlocks[0].DateEarnedHardcore.ShouldNotBeNull();
    }
}
