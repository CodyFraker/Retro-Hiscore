namespace RetroHiscore.Api.Options;

public sealed class TelemetryOptions
{
    public const string SectionName = "Telemetry";

    public bool Enabled { get; set; } = true;

    public string? OtlpEndpoint { get; set; }

    public string ServiceName { get; set; } = "retro-hiscore-api";

    public int GaugeIntervalSeconds { get; set; } = 60;

    public int ExportIntervalSeconds { get; set; } = 60;

    public static void ApplyCollectorEnvironment(IConfiguration configuration, TelemetryOptions options)
    {
        var collectorEnabled = configuration["COLLECTOR_ENABLED"];
        if (!string.IsNullOrWhiteSpace(collectorEnabled)
            && bool.TryParse(collectorEnabled, out var enabled))
        {
            options.Enabled = enabled;
        }

        var frequency = configuration["COLLECTOR_FREQUENCY"];
        if (!string.IsNullOrWhiteSpace(frequency)
            && int.TryParse(frequency, out var seconds)
            && seconds > 0)
        {
            options.GaugeIntervalSeconds = seconds;
            options.ExportIntervalSeconds = seconds;
        }

        var otlp = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(otlp))
        {
            options.OtlpEndpoint = otlp;
        }

        var serviceName = configuration["OTEL_SERVICE_NAME"];
        if (!string.IsNullOrWhiteSpace(serviceName))
        {
            options.ServiceName = serviceName;
        }

        if (options.Enabled && string.IsNullOrWhiteSpace(options.OtlpEndpoint))
        {
            var inContainer = string.Equals(
                configuration["DOTNET_RUNNING_IN_CONTAINER"],
                "true",
                StringComparison.OrdinalIgnoreCase);
            options.OtlpEndpoint = inContainer
                ? "http://otel-collector:4317"
                : "http://localhost:14317";
        }
    }

    public bool IsActive(bool isTesting)
        => !isTesting && Enabled && !string.IsNullOrWhiteSpace(OtlpEndpoint);
}
