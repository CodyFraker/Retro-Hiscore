using Hangfire.Dashboard;

namespace RetroHiscore.Api.Infrastructure;

public sealed class DashboardSecretAuthorizationFilter(string? secret, bool allowWhenSecretUnset) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();
        return DashboardSecretAuthorization.IsAuthorized(http.Request, secret, allowWhenSecretUnset);
    }
}
