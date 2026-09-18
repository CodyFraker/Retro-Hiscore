export function isCollectorEnabled(): boolean {
  const flag = process.env.COLLECTOR_ENABLED;
  if (flag !== undefined && flag !== "") {
    return flag !== "false" && flag !== "0";
  }

  return Boolean(process.env.OTEL_EXPORTER_OTLP_ENDPOINT);
}

export function collectorExportIntervalMs(): number {
  const raw = process.env.COLLECTOR_FREQUENCY ?? process.env.OTEL_METRIC_EXPORT_INTERVAL;
  const seconds = raw ? Number.parseInt(raw, 10) : 60;
  if (!Number.isFinite(seconds) || seconds <= 0) {
    return 60_000;
  }

  return Math.min(Math.max(seconds, 5), 300) * 1000;
}

export function resolveOtlpMetricsEndpoint(): string | undefined {
  if (!isCollectorEnabled()) {
    return undefined;
  }

  const endpoint = process.env.OTEL_EXPORTER_OTLP_ENDPOINT;
  if (!endpoint) {
    return undefined;
  }

  if (endpoint.endsWith("/v1/metrics")) {
    return endpoint;
  }

  return `${endpoint.replace(/\/$/, "")}/v1/metrics`;
}
