import { OTLPMetricExporter } from "@opentelemetry/exporter-metrics-otlp-http";
import { Resource } from "@opentelemetry/resources";
import { MeterProvider, PeriodicExportingMetricReader } from "@opentelemetry/sdk-metrics";
import { metrics } from "@opentelemetry/api";
import { ATTR_SERVICE_NAME } from "@opentelemetry/semantic-conventions";
import {
  collectorExportIntervalMs,
  resolveOtlpMetricsEndpoint,
} from "@/lib/telemetry/collector-env";

let initialized = false;

export function initNodeMetrics() {
  if (initialized || process.env.NODE_ENV === "test") {
    return;
  }

  const endpoint = resolveOtlpMetricsEndpoint();
  if (!endpoint) {
    return;
  }

  const serviceName = process.env.OTEL_SERVICE_NAME ?? "retro-hiscore-web";
  const reader = new PeriodicExportingMetricReader({
    exporter: new OTLPMetricExporter({ url: endpoint }),
    exportIntervalMillis: collectorExportIntervalMs(),
  });

  const provider = new MeterProvider({
    resource: new Resource({
      [ATTR_SERVICE_NAME]: serviceName,
    }),
    readers: [reader],
  });

  metrics.setGlobalMeterProvider(provider);
  initialized = true;
}
