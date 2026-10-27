using Microsoft.AspNetCore.Http;
using RetroHiscore.Api.Infrastructure;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class DashboardSecretAuthorizationTests
{
    [Fact]
    public void IsAuthorized_ReturnsFalse_WhenSecretUnsetAndNotAllowed()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var authorized = DashboardSecretAuthorization.IsAuthorized(context.Request, null, allowWhenSecretUnset: false);

        // Assert
        authorized.ShouldBeFalse();
    }

    [Fact]
    public void IsAuthorized_ReturnsTrue_WhenSecretUnsetAndAllowed()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var authorized = DashboardSecretAuthorization.IsAuthorized(context.Request, null, allowWhenSecretUnset: true);

        // Assert
        authorized.ShouldBeTrue();
    }

    [Fact]
    public void IsAuthorized_ReturnsTrue_WhenHeaderMatches()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[DashboardSecretAuthorization.HeaderName] = "dashboard-secret";

        // Act
        var authorized = DashboardSecretAuthorization.IsAuthorized(
            context.Request,
            "dashboard-secret",
            allowWhenSecretUnset: false);

        // Assert
        authorized.ShouldBeTrue();
    }

    [Fact]
    public void IsAuthorized_ReturnsFalse_WhenOnlyQuerySecretProvided()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?secret=dashboard-secret");

        // Act
        var authorized = DashboardSecretAuthorization.IsAuthorized(
            context.Request,
            "dashboard-secret",
            allowWhenSecretUnset: false);

        // Assert
        authorized.ShouldBeFalse();
    }
}
