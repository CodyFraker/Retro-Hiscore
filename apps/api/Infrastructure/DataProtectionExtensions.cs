using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace RetroHiscore.Api.Infrastructure;

public static class DataProtectionExtensions
{
    public static IServiceCollection AddRetroHiscoreDataProtection(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        bool isTesting)
    {
        if (isTesting)
        {
            services.AddDataProtection();
            return services;
        }

        var keyRingXml = configuration[HostSecurityConfiguration.DataProtectionKeyRingXmlConfigKey]
            ?? configuration[HostSecurityConfiguration.DataProtectionKeyRingXmlEnvKey];

        var builder = services.AddDataProtection()
            .SetApplicationName("RetroHiscore.Api");

        if (!string.IsNullOrWhiteSpace(keyRingXml))
        {
            var xml = Encoding.UTF8.GetString(Convert.FromBase64String(keyRingXml.Trim()));
            services.Replace(ServiceDescriptor.Singleton<IXmlRepository>(new StaticXmlKeyRingRepository(xml)));
        }
        else if (environment.IsDevelopment())
        {
            var keysPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RetroHiscore",
                "DataProtection-Keys");
            Directory.CreateDirectory(keysPath);
            builder.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
        }

        return services;
    }
}

internal sealed class StaticXmlKeyRingRepository(string keyRingXml) : IXmlRepository
{
    public IReadOnlyCollection<XElement> GetAllElements()
        => [XElement.Parse(keyRingXml)];

    public void StoreElement(XElement element, string friendlyName)
        => throw new NotSupportedException("The configured key ring is read-only.");
}
