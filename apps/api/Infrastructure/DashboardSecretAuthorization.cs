namespace RetroHiscore.Api.Infrastructure;

public static class DashboardSecretAuthorization
{
    public const string HeaderName = "X-Dashboard-Secret";

    public static bool IsAuthorized(HttpRequest request, string? secret, bool allowWhenSecretUnset)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            return allowWhenSecretUnset;
        }

        return request.Headers.TryGetValue(HeaderName, out var header)
            && string.Equals(header.ToString(), secret, StringComparison.Ordinal);
    }
}
