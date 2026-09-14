using RetroHiscore.Api.Features.Ra;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class RaApiKeyPoolTests
{
    [Fact]
    public void BuildKeyOrder_DeduplicatesAndPreservesPreferredOrder()
    {
        // Arrange
        var preferred = new[] { "member-a", "shared", "member-a" };
        var memberKeys = new[] { "member-b", "shared" };

        // Act
        var keys = RaApiKeyPool.BuildKeyOrder(preferred, "env-key", memberKeys);

        // Assert
        keys.ShouldBe(["member-a", "shared", "env-key", "member-b"]);
    }

    [Fact]
    public void BuildKeyOrder_SkipsEmptyKeys()
    {
        // Arrange
        var preferred = new[] { "", "  ", "member-a" };

        // Act
        var keys = RaApiKeyPool.BuildKeyOrder(preferred, "", []);

        // Assert
        keys.ShouldBe(["member-a"]);
    }
}
