using RetroHiscore.Api.Features.Dashboard;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class SnapshotDeltaCalculatorTests
{
    [Fact]
    public void ComputeMemberDeltas_ReturnsScoreAndRankDelta_WhenTwoSyncPointsExist()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var series = new List<SnapshotDeltaCalculator.MemberSnapshotSeries>
        {
            new(
                memberId,
                "Shrimp",
                [
                    new SnapshotDeltaCalculator.SnapshotPoint(
                        new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                        100,
                        2,
                        10),
                    new SnapshotDeltaCalculator.SnapshotPoint(
                        new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero),
                        150,
                        1,
                        5)
                ])
        };

        // Act
        var deltas = SnapshotDeltaCalculator.ComputeMemberDeltas(series);

        // Assert
        deltas.Count.ShouldBe(1);
        deltas[0].ScoreDelta.ShouldBe(50);
        deltas[0].FriendRankDelta.ShouldBe(1);
        deltas[0].PreviousFriendRank.ShouldBe(2);
        deltas[0].CurrentFriendRank.ShouldBe(1);
        deltas[0].GlobalRankDelta.ShouldBe(5);
        deltas[0].PreviousGlobalRank.ShouldBe(10);
        deltas[0].CurrentGlobalRank.ShouldBe(5);
    }

    [Fact]
    public void ComputeActionableDeltas_FiltersUnchangedMembers()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var series = new List<SnapshotDeltaCalculator.MemberSnapshotSeries>
        {
            new(
                memberId,
                "Shrimp",
                [
                    new SnapshotDeltaCalculator.SnapshotPoint(
                        new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                        100,
                        1,
                        null),
                    new SnapshotDeltaCalculator.SnapshotPoint(
                        new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero),
                        100,
                        1,
                        null)
                ])
        };

        // Act
        var deltas = SnapshotDeltaCalculator.ComputeActionableDeltas(series);

        // Assert
        deltas.ShouldBeEmpty();
    }
}
