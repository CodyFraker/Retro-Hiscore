using RetroHiscore.Api.Features.Sync;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class SyncRecurringJobCronTests
{
    [Fact]
    public void ForMinuteInterval_ClampsAndReturnsValidCron()
    {
        // Arrange
        // Act
        var cron = SyncRecurringJobCron.ForMinuteInterval(45, 1, 60);

        // Assert
        cron.ShouldNotBeNullOrWhiteSpace();
        SyncRecurringJobCron.Validate(cron);
    }

    [Fact]
    public void ForIntervalDays_ReturnsValidCron()
    {
        // Arrange
        // Act
        var cron = SyncRecurringJobCron.ForIntervalDays(7);

        // Assert
        cron.ShouldBe("0 0 */7 * *");
        SyncRecurringJobCron.Validate(cron);
    }
}
