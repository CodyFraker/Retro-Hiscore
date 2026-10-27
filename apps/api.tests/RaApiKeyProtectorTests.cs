using Microsoft.AspNetCore.DataProtection;
using RetroHiscore.Api.Infrastructure;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class RaApiKeyProtectorTests
{
    [Fact]
    public void Unprotect_ReturnsPlaintext_WhenValueWasStoredUnencrypted()
    {
        // Arrange
        var provider = new EphemeralDataProtectionProvider();
        var protector = new RaApiKeyProtector(provider);
        const string plain = "member-plain-key";

        // Act
        var result = protector.Unprotect(plain);

        // Assert
        result.ShouldBe(plain);
    }

    [Fact]
    public void ProtectAndUnprotect_RoundTripsMemberKey()
    {
        // Arrange
        var provider = new EphemeralDataProtectionProvider();
        var protector = new RaApiKeyProtector(provider);
        const string plain = "member-plain-key";

        // Act
        var stored = protector.Protect(plain);
        var result = protector.Unprotect(stored);

        // Assert
        stored.ShouldNotBe(plain);
        result.ShouldBe(plain);
    }
}
