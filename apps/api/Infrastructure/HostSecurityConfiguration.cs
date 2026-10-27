using Microsoft.Extensions.Hosting;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Infrastructure;

public static class HostSecurityConfiguration
{
    public const string DataProtectionKeyRingXmlConfigKey = "DataProtection:KeyRingXml";
    public const string DataProtectionKeyRingXmlEnvKey = "DataProtection__KeyRingXml";

    public static void ValidateForStartup(IHostEnvironment environment, IConfiguration configuration, bool isTesting)
    {
        if (isTesting)
        {
            return;
        }

        if (!environment.IsProduction())
        {
            return;
        }

        var hangfireSecret = configuration[$"{HangfireDashboardOptions.SectionName}:DashboardSecret"];
        if (string.IsNullOrWhiteSpace(hangfireSecret))
        {
            throw new InvalidOperationException(
                "Hangfire:DashboardSecret is required when ASPNETCORE_ENVIRONMENT is Production.");
        }

        var scalarSecret = configuration[$"{ScalarDashboardOptions.SectionName}:DashboardSecret"];
        if (string.IsNullOrWhiteSpace(scalarSecret))
        {
            throw new InvalidOperationException(
                "Scalar:DashboardSecret is required when ASPNETCORE_ENVIRONMENT is Production.");
        }

        var signInServiceKey = configuration["AUTH_SIGN_IN_SERVICE_KEY"]
            ?? configuration[$"{AuthOptions.SectionName}:SignInServiceKey"];
        if (string.IsNullOrWhiteSpace(signInServiceKey))
        {
            throw new InvalidOperationException(
                "AUTH_SIGN_IN_SERVICE_KEY is required when ASPNETCORE_ENVIRONMENT is Production.");
        }

        var keyRingXml = configuration[DataProtectionKeyRingXmlConfigKey]
            ?? configuration[DataProtectionKeyRingXmlEnvKey];
        if (string.IsNullOrWhiteSpace(keyRingXml))
        {
            throw new InvalidOperationException(
                "DataProtection:KeyRingXml is required when ASPNETCORE_ENVIRONMENT is Production.");
        }
    }

    public static bool AllowsOpenDashboardWhenSecretUnset(IHostEnvironment environment)
        => environment.IsDevelopment() || environment.IsEnvironment("Testing");

    public static bool IsOperationalDocsPublic(IHostEnvironment environment)
        => environment.IsDevelopment() || environment.IsEnvironment("Testing");
}
