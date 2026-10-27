using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using RetroHiscore.Api.Infrastructure;
using Shouldly;

namespace RetroHiscore.Api.Tests;

public class ProductionStartupValidationTests
{
    [Fact]
    public void ValidateForStartup_ThrowsInProduction_WhenDashboardSecretsMissing()
    {
        // Arrange
        var environment = new HostingEnvironment { EnvironmentName = Environments.Production };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act & Assert
        var ex = Should.Throw<InvalidOperationException>(() =>
            HostSecurityConfiguration.ValidateForStartup(environment, configuration, isTesting: false));
        ex.Message.ShouldContain("Hangfire:DashboardSecret");
    }

    [Fact]
    public void ValidateForStartup_DoesNotThrowInDevelopment_WhenDashboardSecretsMissing()
    {
        // Arrange
        var environment = new HostingEnvironment { EnvironmentName = Environments.Development };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act & Assert
        Should.NotThrow(() =>
            HostSecurityConfiguration.ValidateForStartup(environment, configuration, isTesting: false));
    }

    private sealed class HostingEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "RetroHiscore.Api.Tests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
