using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Infrastructure;

public interface IRaApiKeyProtector
{
    string? Protect(string? plainKey);
    string? Unprotect(string? storedKey);
}

public sealed class RaApiKeyProtector(IDataProtectionProvider dataProtectionProvider) : IRaApiKeyProtector
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("RetroHiscore.MemberRaApiKey");

    public string? Protect(string? plainKey)
    {
        if (string.IsNullOrWhiteSpace(plainKey))
        {
            return null;
        }

        return _protector.Protect(plainKey.Trim());
    }

    public string? Unprotect(string? storedKey)
    {
        if (string.IsNullOrWhiteSpace(storedKey))
        {
            return null;
        }

        try
        {
            return _protector.Unprotect(storedKey);
        }
        catch (CryptographicException)
        {
            return storedKey;
        }
    }
}

public static class MemberRaApiKeyExtensions
{
    public static string? GetPlainRaApiKey(this Member member, IRaApiKeyProtector protector)
        => protector.Unprotect(member.RaApiKey);

    public static void SetPlainRaApiKey(this Member member, IRaApiKeyProtector protector, string? plainKey)
        => member.RaApiKey = protector.Protect(plainKey);
}
