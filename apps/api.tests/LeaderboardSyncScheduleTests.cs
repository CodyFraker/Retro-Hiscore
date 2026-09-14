using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class LeaderboardSyncScheduleTests
{
    private static readonly LeaderboardSyncPolicy Policy = new(15, 1440, 168);

    [Fact]
    public void Evaluate_WhenNeverSynced_IsDue()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = LeaderboardSyncSchedule.Evaluate(now, lastSyncedAt: null, maxLastPlayedAt: null, Policy);

        // Assert
        result.Tier.ShouldBe(LeaderboardSyncTier.Cold);
        result.IsDue.ShouldBeTrue();
    }

    [Fact]
    public void Evaluate_WhenHotAndIntervalElapsed_IsDue()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 9, 14, 12, 0, 0, TimeSpan.Zero);
        var lastSynced = now.AddMinutes(-20);
        var played = now.AddHours(-1);

        // Act
        var result = LeaderboardSyncSchedule.Evaluate(now, lastSynced, played, Policy);

        // Assert
        result.Tier.ShouldBe(LeaderboardSyncTier.Hot);
        result.IsDue.ShouldBeTrue();
    }

    [Fact]
    public void Evaluate_WhenHotAndIntervalNotElapsed_IsNotDue()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 9, 14, 12, 0, 0, TimeSpan.Zero);
        var lastSynced = now.AddMinutes(-5);
        var played = now.AddHours(-1);

        // Act
        var result = LeaderboardSyncSchedule.Evaluate(now, lastSynced, played, Policy);

        // Assert
        result.Tier.ShouldBe(LeaderboardSyncTier.Hot);
        result.IsDue.ShouldBeFalse();
        result.NextDueAt.ShouldBe(lastSynced.AddMinutes(15));
    }

    [Fact]
    public void Evaluate_WhenCold_UsesDailyInterval()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 9, 14, 12, 0, 0, TimeSpan.Zero);
        var lastSynced = now.AddHours(-20);
        var played = now.AddDays(-10);

        // Act
        var result = LeaderboardSyncSchedule.Evaluate(now, lastSynced, played, Policy);

        // Assert
        result.Tier.ShouldBe(LeaderboardSyncTier.Cold);
        result.IsDue.ShouldBeFalse();
    }
}
