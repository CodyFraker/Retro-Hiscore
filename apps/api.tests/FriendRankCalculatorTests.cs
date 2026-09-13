using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class FriendRankCalculatorTests
{
    [Fact]
    public void Calculate_RankAsc_OrdersLowestScoreFirst()
    {
        // Arrange
        var scores = new[]
        {
            new FriendRankCalculator.RankableScore(Guid.Parse("11111111-1111-1111-1111-111111111111"), 300),
            new FriendRankCalculator.RankableScore(Guid.Parse("22222222-2222-2222-2222-222222222222"), 100),
            new FriendRankCalculator.RankableScore(Guid.Parse("33333333-3333-3333-3333-333333333333"), 200),
        };

        // Act
        var ranks = FriendRankCalculator.Calculate(scores, rankAsc: true);

        // Assert
        ranks[Guid.Parse("22222222-2222-2222-2222-222222222222")].ShouldBe(1);
        ranks[Guid.Parse("33333333-3333-3333-3333-333333333333")].ShouldBe(2);
        ranks[Guid.Parse("11111111-1111-1111-1111-111111111111")].ShouldBe(3);
    }

    [Fact]
    public void Calculate_RankDesc_OrdersHighestScoreFirst()
    {
        // Arrange
        var scores = new[]
        {
            new FriendRankCalculator.RankableScore(Guid.Parse("11111111-1111-1111-1111-111111111111"), 300),
            new FriendRankCalculator.RankableScore(Guid.Parse("22222222-2222-2222-2222-222222222222"), 100),
        };

        // Act
        var ranks = FriendRankCalculator.Calculate(scores, rankAsc: false);

        // Assert
        ranks[Guid.Parse("11111111-1111-1111-1111-111111111111")].ShouldBe(1);
        ranks[Guid.Parse("22222222-2222-2222-2222-222222222222")].ShouldBe(2);
    }

    [Fact]
    public void Calculate_TiedScores_ShareRank()
    {
        // Arrange
        var a = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var b = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var c = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var scores = new[]
        {
            new FriendRankCalculator.RankableScore(a, 500),
            new FriendRankCalculator.RankableScore(b, 500),
            new FriendRankCalculator.RankableScore(c, 100),
        };

        // Act
        var ranks = FriendRankCalculator.Calculate(scores, rankAsc: false);

        // Assert
        ranks[a].ShouldBe(1);
        ranks[b].ShouldBe(1);
        ranks[c].ShouldBe(3);
    }
}
