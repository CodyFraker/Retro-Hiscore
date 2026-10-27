using Microsoft.AspNetCore.DataProtection;
using RetroHiscore.Api.Infrastructure;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class WebhookUrlProtectorTests
{
    [Fact]
    public void ProtectAndUnprotect_RoundTripsWebhookUrl()
    {
        // Arrange
        var provider = new EphemeralDataProtectionProvider();
        var protector = new WebhookUrlProtector(provider);
        const string url = "https://discord.com/api/webhooks/1/secret-token";

        // Act
        var protectedUrl = protector.Protect(url);
        var plain = protector.Unprotect(protectedUrl);

        // Assert
        protectedUrl.ShouldNotBe(url);
        plain.ShouldBe(url);
    }
}
