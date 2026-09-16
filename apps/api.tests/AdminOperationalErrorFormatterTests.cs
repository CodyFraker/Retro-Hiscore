using RetroHiscore.Api.Infrastructure;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class AdminOperationalErrorFormatterTests
{
    [Fact]
    public void ForAdminDisplay_MapsUndefinedColumnPostgresError()
    {
        // Arrange
        var raw = "42703: column n.ReadyAt does not exist";

        // Act
        var result = AdminOperationalErrorFormatter.ForAdminDisplay(raw);

        // Assert
        result.ShouldBe(AdminOperationalErrorFormatter.SchemaOutOfDateMessage);
    }

    [Fact]
    public void ForAdminDisplay_KeepsShortUserSafeMessage()
    {
        // Arrange
        const string raw = "Webhook alerts: POST failed for outbox abc";

        // Act
        var result = AdminOperationalErrorFormatter.ForAdminDisplay(raw);

        // Assert
        result.ShouldBe(raw);
    }
}
