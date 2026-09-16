using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Dashboard;
using RetroHiscore.Api.Features.Notifications;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class NotificationEventClassifierTests
{
    [Fact]
    public void ClassifyLeaderboardDelta_ReturnsOvertake_WhenFriendRankImproved()
    {
        // Arrange
        var delta = new SnapshotDeltaCalculator.MemberSnapshotDelta(
            Guid.NewGuid(),
            "Player",
            10,
            100,
            110,
            1,
            3,
            2,
            null,
            null,
            null);

        // Act
        var kind = NotificationEventClassifier.ClassifyLeaderboardDelta(delta, hadPreviousSnapshot: true);

        // Assert
        kind.ShouldBe(DiscordNotificationEventKind.LeaderboardFriendOvertake);
    }

    [Fact]
    public void ClassifyLeaderboardDelta_ReturnsNewSubmission_WhenNoPreviousSnapshot()
    {
        // Arrange
        var delta = new SnapshotDeltaCalculator.MemberSnapshotDelta(
            Guid.NewGuid(),
            "Player",
            null,
            null,
            500,
            null,
            null,
            5,
            null,
            null,
            null);

        // Act
        var kind = NotificationEventClassifier.ClassifyLeaderboardDelta(delta, hadPreviousSnapshot: false);

        // Assert
        kind.ShouldBe(DiscordNotificationEventKind.LeaderboardNewSubmission);
    }

    [Fact]
    public void ClassifyLeaderboardDelta_PrefersOvertake_OverNewSubmission()
    {
        // Arrange
        var delta = new SnapshotDeltaCalculator.MemberSnapshotDelta(
            Guid.NewGuid(),
            "Player",
            50,
            null,
            500,
            2,
            null,
            1,
            null,
            null,
            null);

        // Act
        var kind = NotificationEventClassifier.ClassifyLeaderboardDelta(delta, hadPreviousSnapshot: false);

        // Assert
        kind.ShouldBe(DiscordNotificationEventKind.LeaderboardFriendOvertake);
    }
}
