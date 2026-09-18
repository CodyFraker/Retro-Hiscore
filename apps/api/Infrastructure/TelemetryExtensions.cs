using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using RetroHiscore.Api.Options;
using OtlpExportProtocol = OpenTelemetry.Exporter.OtlpExportProtocol;

namespace RetroHiscore.Api.Infrastructure;

public static class TelemetryExtensions
{
    public static IServiceCollection AddRetroHiscoreTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var isTesting = environment.IsEnvironment("Testing");
        services.Configure<TelemetryOptions>(options =>
        {
            configuration.GetSection(TelemetryOptions.SectionName).Bind(options);
            TelemetryOptions.ApplyCollectorEnvironment(configuration, options);
        });

        var telemetryOptions = new TelemetryOptions();
        configuration.GetSection(TelemetryOptions.SectionName).Bind(telemetryOptions);
        TelemetryOptions.ApplyCollectorEnvironment(configuration, telemetryOptions);

        if (!telemetryOptions.IsActive(isTesting))
        {
            return services;
        }

        var exportMs = Math.Clamp(telemetryOptions.ExportIntervalSeconds, 5, 300) * 1000;
        var otlpEndpoint = telemetryOptions.OtlpEndpoint!;

        var otlpUri = new Uri(otlpEndpoint);
        var otlpExporter = new OtlpExporterOptions
        {
            Endpoint = otlpUri,
            Protocol = OtlpExportProtocol.Grpc
        };

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(telemetryOptions.ServiceName))
            .WithMetrics(metrics => metrics
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                .AddMeter("Microsoft.AspNetCore.Routing")
                .AddMeter("System.Net.Http")
                .AddMeter("System.Net.NameResolution")
                .AddMeter("System.Runtime")
                .AddMeter(PlatformMetrics.MeterName)
                .AddReader(new PeriodicExportingMetricReader(
                    new OtlpMetricExporter(otlpExporter),
                    exportMs)));

        services.AddHostedService<PlatformGaugeCollector>();

        return services;
    }
}
