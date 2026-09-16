using Microsoft.AspNetCore.DataProtection;

namespace RetroHiscore.Api.Infrastructure;

public interface IWebhookUrlProtector
{
    string Protect(string plainUrl);
    string Unprotect(string protectedUrl);
}

public sealed class WebhookUrlProtector(IDataProtectionProvider dataProtectionProvider) : IWebhookUrlProtector
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("RetroHiscore.DiscordWebhookUrl");

    public string Protect(string plainUrl)
        => _protector.Protect(plainUrl);

    public string Unprotect(string protectedUrl)
        => _protector.Unprotect(protectedUrl);
}

public static class DiscordWebhookUrlMasking
{
    public static string Mask(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return "";
        }

        var lastSlash = url.LastIndexOf('/');
        if (lastSlash < 0 || lastSlash >= url.Length - 1)
        {
            return "****";
        }

        return url[..(lastSlash + 1)] + "****";
    }
}
