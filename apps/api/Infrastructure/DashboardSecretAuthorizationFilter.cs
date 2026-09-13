using Hangfire.Dashboard;

namespace RetroHiscore.Api.Infrastructure;

public sealed class DashboardSecretAuthorizationFilter(string? secret) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            return true;
        }

        var http = context.GetHttpContext();
        if (http.Request.Headers.TryGetValue("X-Dashboard-Secret", out var header) &&
            string.Equals(header.ToString(), secret, StringComparison.Ordinal))
        {
            return true;
        }

        if (http.Request.Query.TryGetValue("secret", out var query) &&
            string.Equals(query.ToString(), secret, StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }
}
